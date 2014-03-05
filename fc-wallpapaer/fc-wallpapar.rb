
# coding: utf-8
require 'json'
require 'mechanize'

USER_HOME = '/cygdrive/c/Users/Administrator'
SAVE_DIRECTORY = "#{USER_HOME}/Dropbox/Pictures/momoclo/fanclub-calendar/#{Time.now.year}"

# Mechanize 設定
agent = Mechanize.new
agent.user_agent_alias = 'Windows IE 9'
agent.set_proxy('proxy.host', 8080)

# ログイン
login_page = agent.get('https://fc.momoclo.net/pc/login.php')
fc_top_page = login_page.form_with(id: 'loginForm') do |form|
  form.login_id = 'id'
  form.password = 'pass'
end.submit

# 1024x768 の画像保存
image_size_m = fc_top_page.link_with(text: /\A1280.?768\z/).click
image_size_m.save_as("#{SAVE_DIRECTORY}/#{image_size_m.filename}")

# 1920x1080 の画層保存
image_size_l = fc_top_page.link_with(text: /\A1280.?1024\z/).click
image_size_l.save_as("#{SAVE_DIRECTORY}/#{image_size_l.filename}")

# 壁紙の設定

