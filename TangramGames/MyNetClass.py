import socket
import sys

class MyNetClass:

    def __init__(self):
        ip = '10.66.55.214'
        port = 10011
        #port for speech sock
        port_speech = 10010
        self.server_address = (ip, port)
        #self.server_address = ('129.59.79.186', 10010)

        ## might want to use a different port??
        self.speech_address = (ip, port_speech)
        self.BUFFER_SIZE = 1024

    def createConnection(self):

        ### create game server ###
        print "Creating game socket..."
        #create a TCP socket to game as server
        self.game_sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.game_sock.bind(self.server_address)
        self.game_sock.listen(100)
        print "Game socket server binded to port. Listening..."

        ### create speech server ###
        print "Creating speech socket..."
        #create a TCP socket to game as server
        self.speech_sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.speech_sock.bind(self.speech_address)
        self.speech_sock.listen(1)
        print "Speech socket server binded to port. Listening..."


        ### connect both servers ###
        
        #wait for a connection
        self.game_connection, self.game_address = self.game_sock.accept()
        print "Game server - Socket accepted. Connection created."

        #wait for a connection
        self.speech_connection, speech_address = self.speech_sock.accept()
        print "Speech server - Socket accepted. Connection created."



    def sendGameData(self,  data):
        self.game_connection.send(data)

    def decodeSpeechData(self, _str):
        components = _str.split(',')
        return components[0]

    def receiveSpeechData(self):
        data = ''
        speech_response = self.speech_connection.recv(self.BUFFER_SIZE)
        if len(speech_response) > 0:
            data = self.decodeSpeechData(speech_response)
        return data

    def closeSock(self):
        self.speech_sock.close()
        self.game_connection.close()

    def receiveData(self):
        amount_received = 0

        while amount_received<len(str):
            data = self.speech_sock.recv(len(str))
            amount_received += len(data)

if __name__ == '__main__':
    net = MyNetClass()
    net.createConnection()
