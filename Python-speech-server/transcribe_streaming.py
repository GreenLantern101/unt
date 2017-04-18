#!/usr/bin/python
# Copyright (C) 2016 Google Inc.
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
"""Sample that streams audio to the Google Cloud Speech API via GRPC."""

from __future__ import division

### basics
import contextlib
import functools
import re
import signal
import sys

### networking
from MyNetClass import MyNetClass


### speech recognition
import google.auth
import google.auth.transport.grpc
import google.auth.transport.requests
from google.cloud.speech.v1beta1 import cloud_speech_pb2
from google.rpc import code_pb2
import grpc
import pyaudio
from six.moves import queue

# for multithreading
import multiprocessing
from multiprocessing import Process, Manager
import time

# for timer
import threading


'''
NOTE: For actual parallelization in Python, you should use the multiprocessing
module to fork multiple processes that execute in parallel (due to the global
interpreter lock, Python threads provide interleaving but are in fact executed
serially, not in parallel, and are only useful when interleaving I/O operations).
'''

# Audio recording parameters
RATE = 16000
CHUNK = int(RATE / 10)  # 100ms

# The Speech API has a streaming limit of 60 seconds of audio*, so keep the
# connection alive for that long, plus some more to give the API time to figure
# out the transcription.
# * https://g.co/cloud/speech/limits#content

DEADLINE_SECS = 60 * 3 + 15
SPEECH_SCOPE = 'https://www.googleapis.com/auth/cloud-platform'

net = MyNetClass()

# list of processes for parallel computing
processes = []

# queue of messages needed to be sent (shared between processes)
manager = None
messageList = None


def make_channel(host, port):
    """Creates a secure channel with auth credentials from the environment."""
    # Grab application default credentials from the environment
    credentials, _ = google.auth.default(scopes=[SPEECH_SCOPE])

    # Create a secure channel using the credentials.
    http_request = google.auth.transport.requests.Request()
    target = '{}:{}'.format(host, port)

    print "A channel has been made."

    return google.auth.transport.grpc.secure_authorized_channel(
        credentials, http_request, target)


def _audio_data_generator(buff):
    """A generator that yields all available data in the given buffer.

    Args:
        buff - a Queue object, where each element is a chunk of data.
    Yields:
        A chunk of data that is the aggregate of all chunks of data in `buff`.
        The function will block until at least one data chunk is available.
    """
    stop = False
    while not stop:
        # Use a blocking get() to ensure there's at least one chunk of data.
        data = [buff.get()]

        # Now consume whatever other data's still buffered.
        while True:
            try:
                data.append(buff.get(block=False))
            except queue.Empty:
                break

        # `None` in the buffer signals that the audio stream is closed. Yield
        # the final bit of the buffer and exit the loop.
        if None in data:
            stop = True
            data.remove(None)

        yield b''.join(data)


def _fill_buffer(buff, in_data, frame_count, time_info, status_flags):
    """Continuously collect data from the audio stream, into the buffer."""
    buff.put(in_data)
    return None, pyaudio.paContinue


# [START audio_stream]
@contextlib.contextmanager
def record_audio(rate, chunk):
    """Opens a recording stream in a context manager."""
    # Create a thread-safe buffer of audio data
    buff = queue.Queue()

    audio_interface = pyaudio.PyAudio()
    audio_stream = audio_interface.open(
        format=pyaudio.paInt16,
        # The API currently only supports 1-channel (mono) audio
        # https://goo.gl/z757pE
        channels=1, rate=rate,
        input=True, frames_per_buffer=chunk,
        # Run the audio stream asynchronously to fill the buffer object.
        # This is necessary so that the input device's buffer doesn't overflow
        # while the calling thread makes network requests, etc.
        stream_callback=functools.partial(_fill_buffer, buff),
    )

    yield _audio_data_generator(buff)

    audio_stream.stop_stream()
    audio_stream.close()
    # Signal the _audio_data_generator to finish
    buff.put(None)
    audio_interface.terminate()
# [END audio_stream]


def request_stream(data_stream, rate, interim_results=True):
    """Yields `StreamingRecognizeRequest`s constructed from a recording audio
    stream.

    Args:
        data_stream: A generator that yields raw audio data to send.
        rate: The sampling rate in hertz.
        interim_results: Whether to return intermediate results, before the
            transcription is finalized.
    """

    # print "request_stream() called"

    # The initial request must contain metadata about the stream, so the
    # server knows how to interpret it.
    recognition_config = cloud_speech_pb2.RecognitionConfig(
        # There are a bunch of config options you can specify. See
        # https://goo.gl/KPZn97 for the full list.
        encoding='LINEAR16',  # raw 16-bit signed LE samples
        sample_rate=rate,  # the rate in hertz
        # See http://g.co/cloud/speech/docs/languages
        # for a list of supported languages.
        language_code='en-US',  # a BCP-47 language tag
    )
    streaming_config = cloud_speech_pb2.StreamingRecognitionConfig(
        interim_results=interim_results,
        config=recognition_config,
    )

    yield cloud_speech_pb2.StreamingRecognizeRequest(
        streaming_config=streaming_config)

    for data in data_stream:
        # Subsequent requests can all just have the content
        yield cloud_speech_pb2.StreamingRecognizeRequest(audio_content=data)

def task(dialog, messageList):
    # placeholder calculations...
    for i in range(1,100):
        k = 0
        for j in range(1,10**6):
            k+=1
    print "calculations done!"
    # when done, add to sending queue
    # append whatever you want to send
    messageList.append(dialog)

    # needs to return string??
    return dialog

def listen_print_loop(recognize_stream):
    """Iterates through server responses and prints them.

    The recognize_stream passed is a generator that will block until a response
    is provided by the server. When the transcription response comes, print it.

    In this case, responses are provided for interim results as well. If the
    response is an interim one, print a line feed at the end of it, to allow
    the next result to overwrite it, until the response is a final one. For the
    final one, print a newline to preserve the finalized transcription.
    """

    num_chars_printed = 0
    for resp in recognize_stream:

        # restarting stream when current stream expires...
        if resp.error.code != code_pb2.OK:
            main()
            print "--- recognize_stream restarted..."

        # if empty, keep polling
        if not resp.results:
            continue

        # Display the top transcription
        result = resp.results[0]
        transcript = result.alternatives[0].transcript

        # Display interim results, but with a carriage return at the end of the
        # line, so subsequent lines will overwrite them.
        #
        # If the previous result was longer than this one, we need to print
        # some extra spaces to overwrite the previous result
        overwrite_chars = ' ' * max(0, num_chars_printed - len(transcript))

        if not result.is_final:
            sys.stdout.write(transcript + overwrite_chars + '\r')
            sys.stdout.flush()

            num_chars_printed = len(transcript)

        else:
            dialog = str(transcript + overwrite_chars)
            print "FINAL: " + dialog

            # thread for ML classification, before sending
            p = Process(target=task, args=(dialog,messageList))
            p.start()
            processes.append(p)
            # print "Num processes: " + str(len(multiprocessing.active_children()))

            # Exit recognition if any of the transcribed phrases could be
            # one of our keywords.
            if re.search(r'\b(exit|quit)\b', transcript, re.I):
                print "Exiting..."
                break

            num_chars_printed = 0


def main():

    #cleanup ended processes
    for p in processes:
        if not p.is_alive():
            p.join()

    service = cloud_speech_pb2.SpeechStub(
        make_channel('speech.googleapis.com', 443))

    # For streaming audio from the microphone, there are three threads.
    # First, a thread that collects audio data as it comes in
    with record_audio(RATE, CHUNK) as buffered_audio_data:
        # Second, a thread that sends requests with that data
        requests = request_stream(buffered_audio_data, RATE)
        # Third, a thread that listens for transcription responses
        recognize_stream = service.StreamingRecognize(
            requests, DEADLINE_SECS)

        print "recognize_stream made."

        # Exit things cleanly on interrupt
        signal.signal(signal.SIGINT, lambda *_: recognize_stream.cancel())

        # Now, put the transcription responses to use.
        try:
            listen_print_loop(recognize_stream)

            recognize_stream.cancel()
        except grpc.RpcError as e:
            code = e.code()
            # CANCELLED is caused by the interrupt handler, which is expected.
            if code is not code.CANCELLED:
                cleanup()
                raise

# cleanup active proesses, usually when program exits
def cleanup():
    # kill dead processes
    for p in processes:
        if not p.is_alive():
            p.join()
    # kill active processes
    for p in multiprocessing.active_children():
       p.terminate()
    # It is important to join() the process after terminating it in order
    # to give the background machinery time to update the status of the object
    # to reflect the termination.
       p.join()

# timer on separate thread to send all waiting messages on regular interals
def timer_repeat():
  timer = threading.Timer(.5, timer_repeat)
  # by setting this as a daemon thread, automatically
  # killed cleanly when program exits
  timer.setDaemon(True)
  timer.start()
  #########################################################
  # send all messages waiting to be sent
  for i in messageList:
      net.sendGameData(i)

  #clear list
  del messageList[:]
  #########################################################


if __name__ == '__main__':
    cleanup()
    messageList = Manager().list()
    ### create connection
    net.createConnection()

    timer_repeat()
    # print "Num processes: " + str(len(multiprocessing.active_children()))
    main()

    cleanup()
