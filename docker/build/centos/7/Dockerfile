ARG IMAGE_NAME
FROM ${IMAGE_NAME}:latest
LABEL maintainer "Takuya Takeuchi <takuya.takeuchi.dev@gmail.com>"

# install gosu to execute process by non-root user
ENV GOSU_VERSION=1.11
RUN gpg --keyserver hkps://keys.openpgp.org --recv-keys B42F6819007F00F88E364FD4036A9C25BF357DD4 \
    && curl -o /usr/local/bin/gosu -SL "https://github.com/tianon/gosu/releases/download/${GOSU_VERSION}/gosu-amd64" \
    && curl -o /usr/local/bin/gosu.asc -SL "https://github.com/tianon/gosu/releases/download/${GOSU_VERSION}/gosu-amd64.asc" \
    && gpg --verify /usr/local/bin/gosu.asc \
    && rm /usr/local/bin/gosu.asc \
    && rm -r /root/.gnupg/ \
    && chmod +x /usr/local/bin/gosu \
    # Verify that the binary works
    && gosu nobody true

RUN yum update -y --disablerepo=cuda,nvidia-ml && yum install -y \
    git \
 && yum clean all

WORKDIR /opt
RUN wget http://ftp.gnu.org/gnu/gcc/gcc-5.4.0/gcc-5.4.0.tar.gz && tar -xvf gcc-5.4.0.tar.gz && rm gcc-5.4.0.tar.gz
WORKDIR /opt/gcc-5.4.0
RUN ./contrib/download_prerequisites
RUN mkdir build && cd build
RUN ../configure -prefix=/usr/local --enable-checking=release --enable-languages=c,c++ --disable-multilib
RUN make && make install

# copy build script and run
COPY runBuild.sh /runBuild.sh
RUN chmod 744 /runBuild.sh
ENTRYPOINT ["/runBuild.sh"]