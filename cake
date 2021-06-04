#!/bin/sh

log() {
    if [ -t 1 ]; then
        case $1 in
            error) log_type="\e[1mcake: \e[31merror:\e[0m" ;;
            warning) log_type="\e[1mcake: \e[33mwarning:\e[0m" ;;
            info) log_type="\e[1mcake: \e[32minfo:\e[0m" ;;
        esac
    else
        case $1 in
            error) log_type="cake: error:" ;;
            warning) log_type="cake: warning:" ;;
            info) log_type="cake: info:" ;;
        esac
    fi
    shift
    printf "%b %s\n" "$log_type" "$1"
}

in_docker_group() {
    id -Gn | grep -qw docker
}

set_runtime() {
    if type docker > /dev/null && in_docker_group; then
        log info 'using docker'
        runtime=docker
    elif type podman > /dev/null; then
        log info 'using podman'
        runtime=podman
    else
        log error 'cannot use docker or podman'
        exit 1
    fi
}

set_directory() {
    # posixly parse first occurrence of -C dir/--directory dir/--directory=dir
    while getopts ":C:-:" o; do
        # NOTE: we're passing unknown arguments through
        # shellcheck disable=SC2220
        case "$o" in
            C) directory="$OPTARG"; break;;
            -) [ $OPTIND -ge 1 ] && optind=$((OPTIND - 1)) || optind=$OPTIND
               eval option="\$$optind"
               if [ "${option#*=}" != "$option" ]; then
                   # --option=arg style
                   optarg=$(echo "$option" | cut -d '=' -f 2)
                   option=$(echo "$option" | cut -d '=' -f 1)
                   if [ "$option" = '--directory' ]; then
                       directory="$optarg"
                       break
                   fi
               else
                   # --option arg style
                   if [ "$option" = '--directory' ]; then
                       optind=$((optind + 1))
                       eval directory="\$$optind"
                       break
                   fi
               fi;;
        esac
    done

    if [ -z "$directory" ]; then
        directory="$PWD"
    else
        if [ -d "$directory" ]; then
            directory=$(cd "$directory" > /dev/null 2>&1 && echo "$PWD")
        else
            log error "'$directory' is not a directory"
            exit 1
        fi
    fi
}

run_command() {
    set_runtime
    set_directory "$@"

    checksum=$(echo "$directory" | cksum | cut -d ' ' -f 1)
    basename=$(basename "${directory}")
    container="cake-${basename}-${checksum}"

    log info "running 'make $*'"
    log info "in container '$container'"
    log info "in directory '$directory'"

    if "$runtime" build -t "$container" "$directory" > /dev/null; then
        # NOTE: we want `$CAKE_RUNTIME_ARGS` to be split
        # shellcheck disable=SC2086
        "$runtime" run -v "${PWD}:${PWD}" -w "$PWD" --rm -it \
            $CAKE_RUNTIME_ARGS \
            "$container" make "$@"
    fi
}

run_command "$@"
