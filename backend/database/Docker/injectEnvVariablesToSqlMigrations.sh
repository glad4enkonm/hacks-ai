# $1 первым параметром ожидаем папку Migrations

# Проходим по всем файлам в папке Migrations 
for i in `find $1 -name "*.sql" -type f`; do
    echo "$i"
    export THIS_FILE_NAME="$i"
    cat "$i" | envsubst | tee "$i" # добавляем переменные из среды в SQL
done
