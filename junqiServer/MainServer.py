# -*- coding: utf-8 -*-
import socket
import myclient
import json

if __name__ == "__main__":
    s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    s.setsockopt(socket.SOL_SOCKET,socket.SO_REUSEADDR,1)
    s.bind(("0.0.0.0", 6771))
    s.listen(10)
    print("监听中...")
    while True:
        # 客户端1号
        conn1, addr1 = s.accept()
        while True:
            conn1.settimeout(120)
            namestr = conn1.recv(1024)
            namestr = json.loads(namestr.decode())["name"]
            print("客户端1号：" + namestr + "已经连接成功....")
            client1 = myclient.ClientThread(conn1, addr1)
            client1.name = namestr
            client1.start()

            # 客户端2号
            conn2, addr2 = s.accept()
            #处理客户端1号断线，2号转1号
            if not client1.isConnect:
                del client1
                conn1 = conn2
                addr1 = addr2
                continue
            conn2.settimeout(120)
            namestr = conn2.recv(1024)
            namestr = json.loads(namestr.decode())["name"]
            print("客户端2号：" + namestr + "已经连接成功....")
            client2 = myclient.ClientThread(conn2, addr2)
            client2.name = namestr

            # 发送对方连接成功
            jsondict = {'type': 1, 'name': namestr}
            jsonstr = json.dumps(jsondict)
            client1.conn.send(jsonstr.encode())
            jsondict['name'] = client1.name
            jsonstr = json.dumps(jsondict)
            client2.conn.send(jsonstr.encode())

            client2.start()
            client1.opponent = client2
            client2.opponent = client1
            break
