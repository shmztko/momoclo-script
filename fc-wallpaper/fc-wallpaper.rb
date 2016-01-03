# coding: utf-8
require 'logger'
require 'json'
require 'mechanize'
require 'erb'
require 'yaml'
require 'dropbox_sdk'

def save_wallpaper
  # Mechanize 設定
  agent = Mechanize.new
  agent.verify_mode = OpenSSL::SSL::VERIFY_NONE
  agent.user_agent_alias = 'Windows IE 9'
  agent.set_proxy(CONFIG['proxy.host'], CONFIG['proxy.port'], CONFIG['proxy.user'], CONFIG['proxy.pass'])

  # ログイン
  LOGGER.info 'Login to AngelEyes web page'

  login_page = agent.get('https://fc.momoclo.net/pc/login.php')
  fc_top_page = login_page.form_with(id: 'loginForm') do |form|
    form.login_id = CONFIG['angeleyes.id']
    form.password = CONFIG['angeleyes.pass']
  end.submit

  LOGGER.info 'Saving FC wallpaper'
  # 1024x768 の画像保存
  image_size_m = fc_top_page.link_with(text: /\A1280.?768\z/).click
  save_to_dropbox(image_size_m)

  # 1920x1080 の画層保存
  image_size_l = fc_top_page.link_with(text: /\A1280.?1024\z/).click
  save_to_dropbox(image_size_l)

  # 壁紙に設定する画像のパスを返却
  return "#{CONFIG['save.dir']}#{get_separator}#{image_size_l.filename}"
ensure
  agent.shutdown
end

def save_to_dropbox image
  tempfilepath = "#{CONFIG['save.dir']}/#{image.filename}"
  image.save!(tempfilepath)

  client = DropboxClient.new(CONFIG['dropbox.app.token'])
  response = client.put_file("#{CONFIG['dropbox.dir']}/#{image.filename}", open(tempfilepath))
end

def get_separator
  case get_os
  when :windows
    File::ALT_SEPARATOR
  else
    File::Separator
  end
end

def get_os
  return (
    host_os = RbConfig::CONFIG['host_os']
    case host_os
    when /mswin|msys|mingw|cygwin|bccwin|wince|emc/
      :windows
    when /darwin|mac os/
      :macosx
    when /linux/
      :linux
    when /solaris|bsd/
      :unix
    else
      raise Error, "unknown os: #{host_os.inspect}"
    end
  )
end

if __FILE__ == $0

  CONFIG = YAML.load(ERB.new(File.open('config.yml').read).result())

  LOG_FILE = ARGV.length > 0 ? ARGV[0] : 'fc-wallpaper.log'

  LOGGER = Logger.new(LOG_FILE)
  LOGGER.progname = 'fc-wallpaper'
  LOGGER.level = Logger::INFO

  puts save_wallpaper
end
