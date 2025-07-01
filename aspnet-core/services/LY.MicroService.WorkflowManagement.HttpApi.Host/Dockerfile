FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

COPY . /app

EXPOSE 80/tcp
VOLUME [ "./app/Logs" ]
VOLUME [ "./app/Modules" ]

RUN apt update
RUN apt install wget -y

# run run.sh
CMD ["sh", "-c", "chmod +x run.sh && ./run.sh"]