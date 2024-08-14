import socket
import sqlite3

def get_polygon(latitude, longitude):
    # SQLite 데이터베이스 연결
    conn = sqlite3.connect('daejeon.sqlite')
    conn.enable_load_extension(True)
    conn.execute("SELECT load_extension('mod_spatialite');")
    cursor = conn.cursor()

    # WGS84 좌표를 EPSG:5186 좌표로 변환
    cursor.execute("""
        SELECT Transform(ST_GeomFromText(?, 4326), 5186);
    """, (f'POINT({longitude} {latitude})',))
    
    utm_point = cursor.fetchone()
    
    if not utm_point:
        return None
    
    # 특정 좌표를 포함하는 폴리곤 찾기
    cursor.execute("""
        SELECT AsText(GEOMETRY)
        FROM al_d010_30_20240806
        WHERE ST_Contains(GEOMETRY, ?);
    """, (utm_point[0],))

    polygons = cursor.fetchall()
    conn.close()

    return [polygon[0] for polygon in polygons] if polygons else None

def start_server(host='129.254.193.41', port=65432):
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
        s.bind((host, port))
        s.listen()

        print(f"Server listening on {host}:{port}")

        while True:
            conn, addr = s.accept()
            with conn:
                print(f"Connected by {addr}")

                # 데이터를 수신함
                data = conn.recv(1024).decode()
                if not data:
                    break

                # 수신한 데이터는 "latitude,longitude" 형식
                try:
                    latitude, longitude = map(float, data.split(","))
                    polygons = get_polygon(latitude, longitude)

                    if polygons:
                        response = "\n".join(polygons)
                    else:
                        response = "영역내에 해당하는 좌표가 없습니다."
                except ValueError:
                    response = "latitude,longitude 형태를 지켜주세요."

                conn.sendall(response.encode())

if __name__ == "__main__":
    start_server()
