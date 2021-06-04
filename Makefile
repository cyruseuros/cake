PREFIX?=/usr/local
.PHONY=test shell install

test:
	cat /etc/os-release

shell:
	/bin/sh

install:
	@mkdir -p ${DESTDIR}${PREFIX}/bin
	cp -f cake "${DESTDIR}${PREFIX}/bin" && \
	chmod 755 "${DESTDIR}${PREFIX}/bin/cake"
