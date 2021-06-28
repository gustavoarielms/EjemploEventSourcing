FROM rabbitmq:3.8.3-management-alpine

MAINTAINER <gustavoarielms@gmail.com>

COPY rabbitmq.conf /etc/rabbitmq/
COPY definitions.json /etc/rabbitmq/

CMD ["rabbitmq-server"]