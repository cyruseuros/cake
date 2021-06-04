PREFIX?=/usr/local
.PHONY=cat test shell install

cat:
	cat /etc/os-release

shell:
	/bin/sh

# run naked
test:
	./cake cat
	./cake -C subdir
	CAKE_DOCKERFILES='subdir/Dockerfile' ./cake
	CAKE_DOCKERFILES='subdir/example.dockerfile subdir/Dockerfile' ./cake
	CAKE_DOCKERFILES='subdir/example.dockerfile subdir/Dockerfile' ./cake -C subdir

install:
	@mkdir -p ${DESTDIR}${PREFIX}/bin
	cp -f cake "${DESTDIR}${PREFIX}/bin" && \
	chmod 755 "${DESTDIR}${PREFIX}/bin/cake"
