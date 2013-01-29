require 'tools/buildscripts/wixtasks'

namespace :msi do
    candle :x86_candle => "output:x86" do |cndl|
        puts "##teamcity[progressMessage 'Building x86 MSI package']"
        
        cndl.command = "tools/wix-3.7/candle.exe"
        cndl.defines(
            :Name => "Hadouken",
            :BinDir => "build/bin/hdkn-#{BUILD_VERSION}-x86",
            :Platform => :x86,
            :Version => BUILD_VERSION
        )
        cndl.extensions = [ :WixFirewallExtension, :WixUtilExtension ]
        cndl.sources = [ "src/Installer/Installer.wxs", "src/Installer/Core.wxs", "src/Installer/Config.wxs", "src/Installer/Lib.wxs", "src/Installer/Service.wxs" ]
        cndl.out = "src/Installer/"
    end
    
    light :x86_light => "msi:x86_candle" do |lght|
        lght.command = "tools/wix-3.7/light.exe"
        lght.extensions = [ :WixFirewallExtension, :WixUtilExtension ]
        lght.pdbout = "src/Installer/Installer.wixpdb"
        lght.sources = [ "src/Installer/Installer.wixobj", "src/Installer/Core.wixobj", "src/Installer/Config.wixobj", "src/Installer/Lib.wixobj", "src/Installer/Service.wixobj" ]
        lght.out = "build/msi/hdkn-#{BUILD_VERSION}-x86.msi"
    end
    
    task :x86 do
        Rake::Task["msi:x86_light"].invoke
    end
    
    candle :x64_candle => "output:x64" do |cndl|
        puts "##teamcity[progressMessage 'Building x64 MSI package']"
        
        cndl.command = "tools/wix-3.7/candle.exe"
        cndl.defines(
            :Name => "Hadouken",
            :BinDir => "build/bin/hdkn-#{BUILD_VERSION}-x64",
            :Platform => :x64,
            :Version => BUILD_VERSION
        )
        cndl.extensions = [ :WixFirewallExtension, :WixUtilExtension ]
        cndl.sources = [ "src/Installer/Installer.wxs", "src/Installer/Core.wxs", "src/Installer/Config.wxs", "src/Installer/Lib.wxs", "src/Installer/Service.wxs" ]
        cndl.out = "src/Installer/"
    end
    
    light :x64_light => "msi:x64_candle" do |lght|
        lght.command = "tools/wix-3.7/light.exe"
        lght.extensions = [ :WixFirewallExtension, :WixUtilExtension ]
        lght.pdbout = "src/Installer/Installer.wixpdb"
        lght.sources = [ "src/Installer/Installer.wixobj", "src/Installer/Core.wixobj", "src/Installer/Config.wixobj", "src/Installer/Lib.wixobj", "src/Installer/Service.wixobj" ]
        lght.out = "build/msi/hdkn-#{BUILD_VERSION}-x64.msi"
    end
    
    task :x64 do
        Rake::Task["msi:x64_light"].invoke
    end
    
    task :all do
        Rake::Task["msi:x86"].invoke
        Rake::Task["msi:x64"].invoke
    end
end