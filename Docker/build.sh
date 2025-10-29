rm bin/config/*.*
rmdir bin/config
rm -r bin/data/*.*
rmdir -r bin/data
rm bin/logs/*.*
rmdir bin/logs
docker build --tag gehtsoft:fourcdesigner .