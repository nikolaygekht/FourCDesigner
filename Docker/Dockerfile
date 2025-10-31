FROM mcr.microsoft.com/dotnet/aspnet:8.0-azurelinux3.0
WORKDIR /app
COPY ./bin /app
EXPOSE 5000
COPY ./start.sh .
RUN chmod +x start.sh
ENTRYPOINT ["/bin/sh", "./start.sh"]
