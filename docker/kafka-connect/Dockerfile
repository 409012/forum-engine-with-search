FROM quay.io/debezium/connect:2.7.4.Final

ENV KAFKA_LOG4J_OPTS="-Dlog4j.configuration=file:/kafka/config/log4j.properties"

COPY --chown=kafka:kafka worker.properties /kafka/worker.properties
COPY --chown=kafka:kafka fews-source-connector.properties /kafka/fews-source-connector.properties

ENTRYPOINT ["/kafka/bin/connect-standalone.sh"]
CMD ["/kafka/worker.properties", "/kafka/fews-source-connector.properties"]
