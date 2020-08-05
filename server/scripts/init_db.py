import sqlite3

conn = sqlite3.connect('test.db')
c = conn.cursor()

c.execute('''CREATE TABLE IF NOT EXISTS user (
id integer PRIMARY KEY AUTOINCREMENT,
name text NOT NULL,
password text NOT NULL
);''')

test_users = [
    ('netease1', '123'),
    ('netease2', '123'),
    ('netease3', '123')
]
c.executemany('''
INSERT INTO user VALUES (NULL,?,?)
''', test_users)

conn.commit()
conn.close()

