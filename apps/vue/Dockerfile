FROM nginx:latest

# Cài đặt dnsmasq
RUN apt-get update && apt-get install -y dnsmasq

#  Copy vue app vào thư mục /usr/share/nginx/html
COPY ./dist /usr/share/nginx/html
 
COPY ./docker/nginx/nginx.conf /etc/nginx/nginx.conf
COPY ./docker/nginx/default.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

# Khởi động dnsmasq và Nginx
CMD ["sh", "-c", "dnsmasq & nginx -g 'daemon off;'"]
