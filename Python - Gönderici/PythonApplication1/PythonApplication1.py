import cv2
import socket
import struct


host = "0.0.0.0" 
port = 8000

camera = cv2.VideoCapture(0)
server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server.bind((host, port))
server.listen(1)
print("Server baglantisi bekleniyor..")
conn, addr = server.accept()
print(f"Baglant.si kuruldu: {addr}")

try:
    while True:
        ret, frame = camera.read()
        if not ret:
            break  
        frame_resized = cv2.resize(frame, (240,240))
        _, buffer = cv2.imencode('.jpg', frame_resized)
        image_data = buffer.tobytes()
        conn.sendall(struct.pack("<I", len(image_data)))
        conn.sendall(image_data)

except socket.error as e:
    print(f"Soket hatasi: {e}")
except Exception as e:
    print(f"Hata: {e}")
finally:
    camera.release()
    if conn:
        conn.close()
    server.close()