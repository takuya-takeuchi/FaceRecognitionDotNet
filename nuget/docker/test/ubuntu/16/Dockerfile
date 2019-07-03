ARG IMAGE_NAME
FROM ${IMAGE_NAME}:latest
LABEL maintainer "Takuya Takeuchi <takuya.takeuchi.dev@gmail.com>"

# copy build script and run
COPY runTest.sh /runTest.sh
RUN chmod 744 /runTest.sh
ENTRYPOINT ["./runTest.sh"]