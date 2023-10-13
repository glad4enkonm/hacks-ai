# Create a dump for the code database

mysqldump -h 127.0.0.1 -u root -p code > code.sql


# Restore a batabase from dump

1. rm -rf tmpdb/
2. up with db port open
3. mysql -h 127.0.0.1 -u root -p
4. use hack;
5. source hack.sql;