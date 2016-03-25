# -*- coding: utf-8 -*-
import socket
import threading
import time
import json
import copy
import logging

# 判断走棋的情况


def judgeIsAble(fromP, toP, board):
    if board[toP['y']][toP['x']] is 0:
        return 0, 1
    elif board[toP['y']][toP['x']] == 140 and board[fromP['y']][fromP['x']] == 40:
        return 3, 0
    elif board[toP['y']][toP['x']] == 140 and board[fromP['y']][fromP['x']] == 30:
        return 1, 0
    elif board[toP['y']][toP['x']] == 130 and board[fromP['y']][fromP['x']] == 40:
        return 2, 0
    elif board[toP['y']][toP['x']] == 131 and board[fromP['y']][fromP['x']] == 40:
        return 2, -1
    elif (board[toP['y']][toP['x']] - 100) == board[fromP['y']][fromP['x']]:
        return 0, 0
    elif board[toP['y']][toP['x']] == 130 or board[fromP['y']][fromP['x']] == 30:
        return 0, 0
    elif board[toP['y']][toP['x']] == 131 and board[fromP['y']][fromP['x']] != 32:
        return 0, -1
    elif board[fromP['y']][fromP['x']] > (board[toP['y']][toP['x']] - 100):
        return 0, 1
    else:
        return 0, -1

# 转换棋子的方向


def convertChess(fromP, toP):
    tp1 = {"y": 0, "x": 0}
    tp2 = {"y": 0, "x": 0}
    tp1["y"], tp1["x"] = 14 - fromP["y"], 4 - fromP["x"]
    tp2["y"], tp2["x"] = 14 - toP["y"], 4 - toP["x"]
    return tp1, tp2


class ClientThread(threading.Thread):

    def __init__(self, conn, addr):
        self.isConnect = True
        self.conn = conn
        self.addr = addr
        self.opponent = None
        self.isReady = False
        self.skipCount = 0
        self.stepNum = 0
        self.gameOver = False
        self.board = [[0 for col in range(5)] for row in range(15)]
        threading.Thread.__init__(self)

    def run(self):
        while True:
            try:
                temp = self.conn.recv(1024)
                if not temp:
                    self.disconnection()
                    print("数据为空")
                    return
                temp = temp.decode()
                if temp == " ":
                    print(self.name + "心跳包")
                    self.conn.send(" ".encode())
                else:
                    self.dealData(temp)

            except Exception as e:
                #logging.exception(e)
                #print(e)
                self.disconnection()
                print("客户端" + self.name + "断开连接")
                return

    def dealData(self, data):
        print(data)
        jsonstr = json.loads(data)
        # 聊天消息
        if jsonstr["type"] == 10 and self.opponent is not None:
            self.conn.send(data.encode())
            self.opponent.conn.send(data.encode())
        # 一方准备完毕
        if jsonstr["type"] == 2 and self.opponent is not None:
            self.isReady = True
            self.addSelfBoard(jsonstr["board"])
            self.opponent.addOpponentBoard(jsonstr["board"])
            if self.opponent.isReady:
                jsondict = {"type": 3, "which": 1}
                strTp = json.dumps(jsondict)
                self.opponent.conn.send(strTp.encode())
                jsondict["which"] = 0
                strTp = json.dumps(jsondict)
                self.conn.send(strTp.encode())
            else:
                jsondict = {"type": 2}
                strTp = json.dumps(jsondict)
                self.opponent.conn.send(strTp.encode())

        # 走棋
        if jsonstr["type"] == 20 and self.opponent is not None:
            if self.gameOver:
                return
            self.stepNum += 1
            jsondict = {"type": 20, "result": -1, "which": 0, "isAble": -
                        1, "fromP": jsonstr["fromP"], "toP": jsonstr["toP"]}
            jsondict1 = copy.deepcopy(jsondict)
            jsondict1["which"] = 1

            fromP = jsonstr["fromP"]
            toP = jsonstr["toP"]
            if self.board[toP['y']][toP['x']] == 110:
                jsondict["result"] = 1
                jsondict1["result"] = 2
                jsondict1["which"] = 0
                self.gameOver = True
                self.opponent.gameOver = True

            flag, jsondict["isAble"] = judgeIsAble(fromP, toP, self.board)
            jsondict1["isAble"] = jsondict["isAble"]
            self.changeBoard(fromP, toP, jsondict["isAble"])
            tp1, tp2 = convertChess(fromP, toP)
            self.opponent.changeBoard(tp1, tp2, jsondict["isAble"])
            if flag > 0:
                tempY, tempX = self.searchChess(110)
                tempP = {"x": tempX, "y": tempY}
                tempY, tempX = self.opponent.searchChess(110)
                tempP1 = {"x": tempX, "y": tempY}
                if flag == 1:
                    jsondict["tempP"] = tempP
                elif flag == 2:
                    jsondict1["tempP"] = tempP1
                else:
                    jsondict["tempP"] = tempP
                    jsondict1["tempP"] = tempP1

            self.conn.send(json.dumps(jsondict).encode())
            fromP, toP = convertChess(fromP, toP)
            jsondict1["fromP"], jsondict1["toP"] = fromP, toP
            self.opponent.conn.send(json.dumps(jsondict1).encode())

        # 一方投降
        if jsonstr["type"] == 30 and self.opponent is not None:
            if self.gameOver:
                return
            self.gameOver = True
            self.opponent.gameOver = True
            jsondict = {"type": 20, "result": 2}
            self.conn.send(json.dumps(jsondict).encode())
            jsondict["result"] = 1
            self.opponent.conn.send(json.dumps(jsondict).encode())

        # 一方求和
        if jsonstr["type"] == 40 and self.opponent is not None:
            if self.gameOver:
                return
            if jsonstr["which"] is 0:
                jsondict = {"type": 40, "which": 1, "result": self.stepNum}
                self.opponent.conn.send(json.dumps(jsondict).encode())
            elif abs(jsonstr["result"] - self.stepNum) <= 1:
                self.gameOver = True
                self.opponent.gameOver = True
                jsondict = {"type": 20, "result": 0, "which": 0}
                self.conn.send(json.dumps(jsondict).encode())
                self.opponent.conn.send(json.dumps(jsondict).encode())

        # 一方跳过
        if jsonstr["type"] == 50 and self.opponent is not None:
            if self.gameOver:
                return
            self.skipCount += 1
            self.stepNum += 1
            jsondict = {"type": 50, "which": 0, "result": 0,
                        "name": self.name, "isAble": self.skipCount}
            if self.skipCount >= 5:
                self.gameOver = True
                self.opponent.gameOver = True
                jsondict["result"] = 2
                self.conn.send(json.dumps(jsondict).encode())
                jsondict["result"] = 1
                self.opponent.conn.send(json.dumps(jsondict).encode())
            else:
                self.conn.send(json.dumps(jsondict).encode())
                jsondict["which"] = 1
                self.opponent.conn.send(json.dumps(jsondict).encode())

    # 加入己方棋子
    def addSelfBoard(self, board):
        for y in range(6):
            for x in range(5):
                self.board[y][x] = board[y * 5 + x]

    # 加入对方棋子
    def addOpponentBoard(self, board):
        for y in range(6):
            for x in range(5):
                self.board[14 - y][-(x + 1)] = board[y * 5 + x] + 100

    # 发送游戏结束消息
    def sendWinMsg(self, which):
        jsondict = {"type": 100, "which": which}
        jsonstr = json.dumps(jsondict)
        self.conn.send(jsonstr.encode())

    def searchChess(self, value):
        for y in range(15):
            for x in range(5):
                if self.board[y][x] == value:
                    return y, x

    def changeBoard(self, fromP, toP, isAble):
        if isAble == 0:
            self.board[fromP['y']][fromP['x']] = 0
            self.board[toP['y']][toP['x']] = 0
        elif isAble == 1:
            self.board[toP['y']][toP['x']] = self.board[fromP['y']][fromP['x']]
            self.board[fromP['y']][fromP['x']] = 0
        else:
            self.board[fromP['y']][fromP['x']] = 0

    # 处理连接中断
    def disconnection(self):
        self.conn.close()
        self.isConnect = False
        try:
            if self.opponent is not None and self.opponent.isConnect:
                jsondict = {"type": 20, "result": 3}
                self.opponent.conn.send(json.dumps(jsondict).encode())
        except Exception as e:
            print(e)
