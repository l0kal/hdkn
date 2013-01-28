require "rubygems"
require "bundler"
Bundler.setup
$: << './'

require 'albacore'
require 'semver'
require 'rake/clean'

require 'tools/buildscripts/environment'
require 'tools/buildscripts/utils'
require 'tools/buildscripts/wixtasks'

CLOBBER.include("build/*")

task :default => [ :alpha ]

task :alpha => [ :clobber, "env:alpha", "env:release", :build_x86, :reset, :build_x64 ]
task :beta => [ :clobber, "env:beta", "env:release", :build_x86, :reset, :build_x64 ]
task :rc => [ :clobber, "env:rc", "env:release", :build_x86, :reset, :build_x64 ]
task :ga => [ :clobber, "env:ga", "env:release", :build_x86, :reset, :build_x64 ]

task :build_x86 => [ "arch:x86", :version, :build, :test, :output, :zip_webui, :zip, :msi ]
task :build_x64 => [ "arch:x64", :version, :build, :test, :output, :zip_webui, :zip, :msi ]

task :reset do
    Rake::Task["version"].reenable
    Rake::Task["build"].reenable
    Rake::Task["test"].reenable
    Rake::Task["test_nunit"].reenable
    Rake::Task["test_teamcity"].reenable
    Rake::Task["output"].reenable
    Rake::Task["zip_webui"].reenable
    Rake::Task["zip"].reenable
    Rake::Task["msi"].reenable
    Rake::Task["msi_candle_installer"].reenable
    Rake::Task["msi_light_installer"].reenable
    Rake::Task["msi_candle_bundle"].reenable
    Rake::Task["msi_light_bundle"].reenable
end

desc "Build"
msbuild :build => :version do |msb|
    puts "##teamcity[progressMessage 'Compiling code']"
    
    msb.properties :configuration => "Release"
    msb.properties :platform      => BUILD_PLATFORM
    msb.targets :Clean, :Build
    
    msb.solution = "Hadouken.sln"
end

desc "Versioning"
assemblyinfo :version => "env:common" do |asm|    
    asm.version = BUILD_VERSION
    asm.file_version = BUILD_VERSION
    
    asm.company_name = "Hadouken"
    asm.product_name = "Hadouken"
    asm.copyright = "2012"
    asm.namespaces = "System", "System.Reflection", "System.Runtime.InteropServices", "System.Security"
    
    asm.custom_attributes :AssemblyInformationalVersion => "#{BUILD_VERSION} (#{BUILD_PLATFORM})", # disposed as product version in explorer
        :CLSCompliantAttribute => false,
        :AssemblyConfiguration => "#{CONFIGURATION}"
    
    asm.com_visible = false
    
    asm.output_file = "src/Shared/CommonAssemblyInfo.cs"
end

desc "Test"
task :test => :build do
    puts "##teamcity[progressMessage 'Running unit tests']"
    FileUtils.mkdir_p "build/reports" unless FileTest.exists?("build/reports")
    
    if(ENV['TEAMCITY_VERSION'])
        Rake::Task["test_teamcity"].invoke
    else
        Rake::Task["test_nunit"].invoke
    end
end

task :test_teamcity => :build do
    puts "#{ENV['NUNIT_LAUNCHER']} v4.0 #{BUILD_PLATFORM} NUnit-2.6.0 src/Tests/Hadouken.UnitTests/bin/#{BUILD_PLATFORM}/#{CONFIGURATION}/Hadouken.UnitTests.dll"
    system "#{ENV['NUNIT_LAUNCHER']} v4.0 #{BUILD_PLATFORM} NUnit-2.6.0 src/Tests/Hadouken.UnitTests/bin/#{BUILD_PLATFORM}/#{CONFIGURATION}/Hadouken.UnitTests.dll"
end

task :test_nunit => :build do
    nunitcmd = "tools/nunit-2.6.0.12051/bin/nunit-console-x86.exe"

    if BUILD_PLATFORM == "x64"
        nunitcmd = "tools/nunit-2.6.0.12051/bin/nunit-console.exe"
    end
    system "#{nunitcmd} /framework:v4.0.30319 /xml:build/reports/nunit.xml src/Tests/Hadouken.UnitTests/bin/#{BUILD_PLATFORM}/#{CONFIGURATION}/Hadouken.UnitTests.dll"
end

desc "Output"
task :output => :build do
    puts "##teamcity[progressMessage 'Outputting binaries']"
    
    copy_files "src/Hosts/Hadouken.Hosts.WindowsService/bin/#{BUILD_PLATFORM}/#{CONFIGURATION}/", "*.{dll,exe}", "build/#{BUILD_PLATFORM}/hdkn-#{BUILD_VERSION}-#{BUILD_PLATFORM}"
    copy_files "src/Hosts/Hadouken.Hosts.WindowsService/bin/#{BUILD_PLATFORM}/#{CONFIGURATION}/#{BUILD_PLATFORM}/", "*.{dll,exe}", "build/#{BUILD_PLATFORM}/hdkn-#{BUILD_VERSION}-#{BUILD_PLATFORM}"
    
    copy_files "src/Config/#{CONFIGURATION}/", "*.{config}", "build/#{BUILD_PLATFORM}/hdkn-#{BUILD_VERSION}-#{BUILD_PLATFORM}"
end

desc "Zip"
zip :zip => :output do |zip|
    zip.directories_to_zip "build/#{BUILD_PLATFORM}/hdkn-#{BUILD_VERSION}-#{BUILD_PLATFORM}"
    zip.output_file = "hdkn-#{BUILD_VERSION}-#{BUILD_PLATFORM}.zip"
    zip.output_path = "#{File.dirname(__FILE__)}/build/#{BUILD_PLATFORM}/"
end

desc "Zip WebUI"
zip :zip_webui do |zip|
    zip.directories_to_zip "src/WebUI"
    zip.output_file = "webui.zip"
    zip.output_path = "#{File.dirname(__FILE__)}/build/#{BUILD_PLATFORM}/hdkn-#{BUILD_VERSION}-#{BUILD_PLATFORM}/"
end

task :msiold => :output do
    system "tools/wix-3.7/candle.exe -ext WixFirewallExtension -ext WixUtilExtension -dPlatform=#{BUILD_PLATFORM} -dBuildVersion=#{BUILD_VERSION} -dBinDir=build/#{BUILD_PLATFORM}/hdkn-#{BUILD_VERSION}-#{BUILD_PLATFORM} -out src/Installer/ src/Installer/Hadouken.wxs src/Installer/WinSrvConfig.wxs src/Installer/WebUIConfig.wxs src/Installer/IncorrectData.wxs"
    system "tools/wix-3.7/light.exe -ext WixUIExtension -ext WixFirewallExtension -ext WixUtilExtension -sval -pdbout src/Installer/Hadouken.wixpdb -out build/#{BUILD_PLATFORM}/hdkn-#{BUILD_VERSION}-#{BUILD_PLATFORM}.msi src/Installer/Hadouken.wixobj src/Installer/WinSrvConfig.wixobj src/Installer/WebUIConfig.wixobj src/Installer/IncorrectData.wixobj"

    system "tools/wix-3.7/candle.exe -dPlatform=#{BUILD_PLATFORM} -dVersion=#{BUILD_VERSION} -dName=Hadouken -out src/Installer/ src/Installer/Bundle.wxs"
end

task :msi do
    Rake::Task["msi_candle_installer"].invoke
    Rake::Task["msi_light_installer"].invoke
    Rake::Task["msi_candle_bundle"].invoke
    Rake::Task["msi_light_bundle"].invoke
end

desc "MSI (candle)"
candle :msi_candle_installer do |cndl|
    cndl.command = "tools/wix-3.7/candle.exe"
    cndl.defines(
        :Name => "Hadouken",
        :BinDir => "build/#{BUILD_PLATFORM}/hdkn-#{BUILD_VERSION}-#{BUILD_PLATFORM}",
        :Platform => BUILD_PLATFORM,
        :Version => BUILD_VERSION
    )
    cndl.extensions = [ :WixFirewallExtension, :WixUtilExtension ]
    cndl.sources = [ "src/Installer/Installer.wxs", "src/Installer/Core.wxs" ]
    cndl.out = "src/Installer/"
end

desc "MSI (light)"
light :msi_light_installer do |lght|
    lght.command = "tools/wix-3.7/light.exe"
    lght.extensions = [ :WixFirewallExtension, :WixUtilExtension ]
    lght.pdbout = "src/Installer/Installer.wixpdb"
    lght.sources = [ "src/Installer/Installer.wixobj", "src/Installer/Core.wixobj" ]
    lght.out = "build/#{BUILD_PLATFORM}/hdkn-#{BUILD_VERSION}-#{BUILD_PLATFORM}.msi"
end

desc "Bundle (candle)"
candle :msi_candle_bundle do |cndl|
    cndl.command = "tools/wix-3.7/candle.exe"
    cndl.defines(
        :Name => "Hadouken",
        :InstallerResources => "src/Installer/Hadouken.Installer/bin/#{CONFIGURATION}",
        :MsiPackage => "build/#{BUILD_PLATFORM}/hdkn-#{BUILD_VERSION}-#{BUILD_PLATFORM}.msi",
        :Platform => BUILD_PLATFORM,
        :Version => BUILD_VERSION
    )
    cndl.extensions = [ :WixUtilExtension, :WixBalExtension ]
    cndl.sources = [ "src/Installer/Bundle.wxs" ]
    cndl.out = "src/Installer/"
end

desc "Bundle (light)"
light :msi_light_bundle do |lght|
    lght.command = "tools/wix-3.7/light.exe"
    lght.extensions = [ :WixUtilExtension, :WixBalExtension ]
    lght.pdbout = "src/Installer/Bundle.wixpdb"
    lght.sources = [ "src/Installer/Bundle.wixobj" ]
    lght.out = "build/#{BUILD_PLATFORM}/hdkn-#{BUILD_VERSION}-#{BUILD_PLATFORM}.exe"
end