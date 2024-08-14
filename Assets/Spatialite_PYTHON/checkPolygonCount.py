'''
SELECT
    rowid AS index_id,  -- 레코드의 인덱스(또는 ID)
    AsText(GEOMETRY) AS geometry_text,  -- 폴리곤의 WKT 좌표
    ST_NumGeometries(GEOMETRY) AS num_geometries  -- 폴리곤의 개수
FROM
    al_d010_30_20240806
WHERE
    ST_NumGeometries(GEOMETRY) > 1;  -- 폴리곤이 2개 이상인 경우만 필터링


'''

import sqlite3

conn = sqlite3.connect('seoul.sqlite')
conn.enable_load_extension(True)
conn.execute("SELECT load_extension('mod_spatialite');")
cursor = conn.cursor()

cursor.execute(
'''
SELECT
    rowid AS index_id,  -- 레코드의 인덱스(또는 ID)
    AsText(GEOMETRY) AS geometry_text,  -- 폴리곤의 WKT 좌표
    ST_NumGeometries(GEOMETRY) AS num_geometries  -- 폴리곤의 개수
FROM
    al_d010_11_20240706
WHERE
    ST_NumGeometries(GEOMETRY) > 1;  -- 폴리곤이 2개 이상인 경우만 필터링
'''
)

datas = cursor.fetchall()
for data in datas :
    print(data[0])