

namespace :setup do
    candle :candle => "msi:all" do |cndl|
        puts "##teamcity[progressMessage 'Compiling setup bundle']"
        
        cndl.command = "tools/wix-3.7/candle.exe"
        cndl.defines(
            :Name => "Hadouken",
            :InstallerResources => "src/Installer/Hadouken.Installer/bin/#{CONFIGURATION}",
            :MsiPackage_x86 => "build/msi/hdkn-#{BUILD_VERSION}-x86.msi",
            :MsiPackage_x64 => "build/msi/hdkn-#{BUILD_VERSION}-x64.msi",
            :Version => BUILD_VERSION
        )
        cndl.extensions = [ :WixUtilExtension, :WixBalExtension ]
        cndl.sources = [ "src/Installer/Bundle.wxs" ]
        cndl.out = "src/Installer/"
    end
    
    light :light => "setup:candle" do |lght|
        puts "##teamcity[progressMessage 'Linking setup bundle']"
        
        lght.command = "tools/wix-3.7/light.exe"
        lght.extensions = [ :WixUtilExtension, :WixBalExtension ]
        lght.pdbout = "src/Installer/Bundle.wixpdb"
        lght.sources = [ "src/Installer/Bundle.wixobj" ]
        lght.out = "build/setup/hdkn-#{BUILD_VERSION}.exe"
    end
    
    task :all do
        Rake::Task["setup:candle"].invoke
        Rake::Task["setup:light"].invoke
    end
end