ARG IMAGE_NAME
FROM ${IMAGE_NAME}:latest
LABEL maintainer "Takuya Takeuchi <takuya.takeuchi.dev@gmail.com>"

# install gosu to execute process by non-root user
ENV GOSU_VERSION=1.11
RUN gpg --keyserver ha.pool.sks-keyservers.net --recv-keys B42F6819007F00F88E364FD4036A9C25BF357DD4 \
    && curl -o /usr/local/bin/gosu -SL "https://github.com/tianon/gosu/releases/download/${GOSU_VERSION}/gosu-amd64" \
    && curl -o /usr/local/bin/gosu.asc -SL "https://github.com/tianon/gosu/releases/download/${GOSU_VERSION}/gosu-amd64.asc" \
    && gpg --verify /usr/local/bin/gosu.asc \
    && rm /usr/local/bin/gosu.asc \
    && rm -r /root/.gnupg/ \
    && chmod +x /usr/local/bin/gosu \
    # Verify that the binary works
    && gosu nobody true

# install package to build
RUN yum install -y https://github.com/PowerShell/PowerShell/releases/download/v6.2.0/powershell-6.2.0-1.rhel.7.x86_64.rpm

# copy build script and run
COPY runTest.sh /runTest.sh
RUN chmod 744 /runTest.sh
ENTRYPOINT ["./runTest.sh"]