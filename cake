#!/bin/sh

log() {
    if [ -t 1 ]; then
        case $1 in
            error) log_type="\e[1mcake: \e[32merror:\e[0m" ;;
            warning) log_type="\e[1mcake: \e[32mwarning:\e[0m" ;;
            info) log_type="\e[1mcake: \e[32minfo:\e[0m" ;;
        esac
    else
        case $1 in
            error) log_type="cake: error:" ;;
            warning) log_type="cake: warning:" ;;
            info) log_type="cake: info:" ;;
        esac
    fi
    shift 1
    echo -e $log_type $@
}

in_docker_group() {
    id -Gn | grep -qw docker
}

set_runtime() {
    if type docker > /dev/null && in_docker_group; then
        log info using docker
        runtime=docker
    elif type podman > /dev/null; then
        log info using podman
        runtime=podman
    else
        log error cannot use docker or podman
        exit 1
    fi
}

set_directory() {
    # posixly parse first occurrence of -C dir/--directory=dir
    while getopts ":C:-:" o; do
        case "$o" in
            C) directory="$OPTARG"; break;;
            -) [ $OPTIND -ge 1 ] && optind=`expr $OPTIND - 1` || optind=$OPTIND
                eval OPTION="\$$optind"
                OPTARG=`echo $OPTION | cut -d '=' -f 2`
                OPTION=`echo $OPTION | cut -d '=' -f 1`
                case $OPTION in
                    --directory) directory="$OPTARG"; break;;
                esac
                OPTIND=1
                shift;;
        esac
    done

    if [ -z "$directory" ]; then
        directory="$PWD"
    else
        directory=`realpath ${directory}`
    fi
}

run_command() {
    set_runtime
    set_directory $@

    inode=`ls -id ${directory} | cut -d ' ' -f 1`
    basename=`basename "${directory}"`
    container="${basename}-${inode}"

    log info running "'make $@'"
    log info in container "'$container'"
    log info in directory "'$directory'"

    $runtime build -t "$container" "$directory" > /dev/null && \
        $runtime run \
        -v "${PWD}:${PWD}" \
        -w "${PWD}" \
        --rm -it \
        $CAKE_RUNTIME_ARGS \
        $container make $@
}

run_command $@
