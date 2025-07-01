FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

COPY . /app

# Make run.sh executable during build
RUN chmod +x /app/run.sh

EXPOSE 80/tcp
VOLUME [ "./app/Logs" ]
VOLUME [ "./app/Modules" ]

RUN apt update
RUN apt install wget -y

# Run the script directly
CMD ["./run.sh"]