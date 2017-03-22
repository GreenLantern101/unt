# Objective:
Implement an asynchronous multithreaded application such that speech input is continuously being fed in a stream, while the past speech data gets processed by a machine-learning classification algorithm simultaneously.

### What to use?
Can't use threading because due to GIL, doesn't take advantage of hardware, too slow.
* Use multiprocessing module to get around GIL.

Can't use multiprocessing's Pool.apply_async b/c AsyncResult.get() is blocking.
* Use multiprocessing's Process().

### Obstacles:
1. Multiprocessing means global variables are not shared between processes, can't simply pass as parameters. This resulted in not being able to call net.sendGameData(dialog) in each process because socket and TCP streams are not pickleable.
* Workaround: have each child process write to shared memory held by parent process (using multiprocessing.Manager)

2. Need to send data to Unity instance while computing/getting more speech data.
* Use a threading.Timer to send data at regular intervals, with thread being set as a daemon thread.
