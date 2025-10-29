FROM mcr.microsoft.com/dotnet/aspnet:8.0.21-alpine3.22-icu
RUN apk update
WORKDIR /app
COPY ./bin /app
EXPOSE 5000
ENTRYPOINT ["dotnet", "Gehtsoft.FourCDesigner.dll"]
