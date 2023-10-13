#!/bin/bash
# Загружаем переменные среды для запуска
if [[ -f .env ]]; then
  set -o allexport
  source .env
  set +o allexport
  echo .env imported
fi

db_execute_sql()
{
  sql=$1
  echo $(mysql -h 127.0.0.1 -u root --password=$MYSQL_ROOT_PASSWORD --database=$DATABASE_NAME -s --execute="${sql}")
}

cd ../database || exit

lastMigrationNumberInDB=$(db_execute_sql "use $MYSQL_DATABASE; select MigrationId from Migration ORDER BY MigrationId desc limit 1;")
lastMigrationNumberInDB=$(("0$lastMigrationNumberInDB"))
echo "Last migration in DB = $lastMigrationNumberInDB"


# Проходим по всем файлам в папке Migrations 
filesCount=1
for i in $(find ./MigrationsWithEnv -name "*.sql" -type f | sort); do
    echo "$i"
    
    if [[ "$filesCount" -gt $lastMigrationNumberInDB ]]; then
      echo "-> DB"
      export THIS_FILE_NAME="$i"
      sqlContent=$(cat "$i")
      (db_execute_sql "$sqlContent")
    fi
    filesCount=$((filesCount+1))
done
