 # Загружаем переменные среды для запуска
if [[ -f .env ]]; then
  set -o allexport
  source .env
  set +o allexport
  echo .env imported
fi

# Обновляем версию репозитория
#git pull || exit 1 
cd ../database
#docker compose down # останавливаем все сервисы, если есть
# Удаляем существующую папку миграций если есть
rm -rf MigrationsWithEnv \
&& cp -r Migrations MigrationsWithEnv \
&& bash ./Docker/injectEnvVariablesToSqlMigrations.sh ./MigrationsWithEnv
#&& cd ../deploy \
#&& docker compose up --build # перезапускаем
