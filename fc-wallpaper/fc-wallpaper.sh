#!/bin/bash
SCRIPT_DIR=$(cd $(dirname $0);pwd)
LOG_FILE=$SCRIPT_DIR/fc-wallpaper.log
cd $SCRIPT_DIR

export PATH="$HOME/.anyenv/bin:$PATH:$HOME/bin"
eval "$(anyenv init -)"

bundle exec ruby fc-wallpaper.rb ${LOG_FILE}
