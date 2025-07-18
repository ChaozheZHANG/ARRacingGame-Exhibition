# 展览级AR赛车游戏 - 服务器部署版
FROM nginx:alpine

# 复制展览优化的静态文件
COPY ./build /usr/share/nginx/html
COPY ./nginx.conf /etc/nginx/nginx.conf

# 展览优化设置
RUN apk add --no-cache \
    gzip \
    brotli \
    && rm -rf /var/cache/apk/*

# 创建展览用户
RUN addgroup -g 1001 -S exhibition && \
    adduser -S -D -H -u 1001 -h /var/cache/nginx -s /sbin/nologin -G exhibition -g exhibition exhibition

# 优化Nginx配置用于AR展览
COPY docker-config/nginx-exhibition.conf /etc/nginx/conf.d/default.conf

# 健康检查
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost/ || exit 1

EXPOSE 80
EXPOSE 443

CMD ["nginx", "-g", "daemon off;"] 