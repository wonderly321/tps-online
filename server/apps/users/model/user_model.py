from service.db import DB

def fetch_user_by_name(name):
    ret = DB.execute('SELECT * FROM user WHERE name="%s"'%(name))
    return ret[0] if ret else None

def insert_new_user(name, pwd):
    ret = DB.execute('''INSERT INTO user VALUES (NULL,"%s","%s");'''%(name,pwd))
    ret = fetch_user_by_name(name)
    return ret