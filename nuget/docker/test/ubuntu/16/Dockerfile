ARG IMAGE_NAME
FROM ${IMAGE_NAME}:latest
LABEL maintainer "Takuya Takeuchi <takuya.takeuchi.dev@gmail.com>"

# install package to build
RUN apt-get update && apt-get install -y \
    libgdiplus \
 && apt-get clean && rm -rf /var/lib/apt/lists/*

# copy build script and run
COPY runTest.sh /runTest.sh
RUN chmod 744 /runTest.sh
ENTRYPOINT ["./runTest.sh"]