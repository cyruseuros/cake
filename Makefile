PREFIX?=/usr/local
.PHONY=cat shell install test test-basic test-directory test-dockerfiles test-find-dockerfiles

cat:
	cat /etc/os-release

shell:
	/bin/sh

# run naked
test: test-basic test-directory test-dockerfiles test-find-dockerfiles

test-basic:
	./cake
	./cake cat

test-directory:
	./cake -C subdir
	./cake --directory subdir
	./cake --directory=subdir

test-dockerfiles:
	CAKE_DOCKERFILES='subdir/Dockerfile' ./cake
	CAKE_DOCKERFILES='subdir/example.dockerfile subdir/Dockerfile' ./cake -C subdir

test-find-dockerfiles:
	CAKE_DOCKERFILES='subdir/' ./cake

install:
	@mkdir -p ${DESTDIR}${PREFIX}/bin
	cp -f cake "${DESTDIR}${PREFIX}/bin" && \
	chmod 755 "${DESTDIR}${PREFIX}/bin/cake"
