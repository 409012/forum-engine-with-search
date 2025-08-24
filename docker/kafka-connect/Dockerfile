FROM confluentinc/cp-kafka-connect-base:7.7.1

RUN confluent-hub install --no-prompt confluentinc/kafka-connect-jdbc:10.7.4

COPY worker.properties .
COPY fews-source-connector.properties .

CMD ["/bin/connect-standalone", "worker.properties", "fews-source-connector.properties"]