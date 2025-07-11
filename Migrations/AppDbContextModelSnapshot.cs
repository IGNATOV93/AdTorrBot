﻿// <auto-generated />
using System;
using AdTorrBot.BotTelegram.Db;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace AdTorrBot.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.0");

            modelBuilder.Entity("AdTorrBot.BotTelegram.Db.Model.SettingsBot", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("IdChat")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("LastChangeUid")
                        .HasColumnType("TEXT");

                    b.Property<double>("TimeZoneOffset")
                        .HasColumnType("REAL");

                    b.HasKey("Id");

                    b.ToTable("SettingsBot");
                });

            modelBuilder.Entity("AdTorrBot.BotTelegram.Db.Model.SettingsTorrserverBot", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("AutoBackupTime")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsActiveAutoChange")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsAutoBackupEnabled")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsTorrserverAutoRestart")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Login")
                        .HasColumnType("TEXT");

                    b.Property<string>("Password")
                        .HasColumnType("TEXT");

                    b.Property<string>("TimeAutoChangePassword")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("TorrserverRestartTime")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("idChat")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("SettingsTorrserverBot");
                });

            modelBuilder.Entity("AdTorrBot.BotTelegram.Db.Model.TextInputFlag", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("FlagLogin")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("FlagLoginPasswordOtherProfile")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("FlagNewLoginAndPasswordOtherProfile")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("FlagNoteOtherProfile")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("FlagPassword")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("FlagServerArgsSettLogPath")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("FlagServerArgsSettPath")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("FlagServerArgsSettPort")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("FlagServerArgsSettPubIPv4")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("FlagServerArgsSettPubIPv6")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("FlagServerArgsSettSslCert")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("FlagServerArgsSettSslKey")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("FlagServerArgsSettSslPort")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("FlagServerArgsSettTorrentAddr")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("FlagServerArgsSettTorrentsDir")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("FlagServerArgsSettWebLogPath")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("FlagTorrSettCacheSize")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("FlagTorrSettConnectionsLimit")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("FlagTorrSettDownloadRateLimit")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("FlagTorrSettFriendlyName")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("FlagTorrSettPeersListenPort")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("FlagTorrSettPreloadCache")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("FlagTorrSettReaderReadAHead")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("FlagTorrSettRetrackersMode")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("FlagTorrSettSslCert")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("FlagTorrSettSslKey")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("FlagTorrSettSslPort")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("FlagTorrSettTorrentDisconnectTimeout")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("FlagTorrSettTorrentsSavePath")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("FlagTorrSettUploadRateLimit")
                        .HasColumnType("INTEGER");

                    b.Property<string>("IdChat")
                        .HasColumnType("TEXT");

                    b.Property<string>("LastTextFlagTrue")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("TextInputFlag");
                });

            modelBuilder.Entity("AdTorrBot.BotTelegram.Db.Model.TorrserverModel.BitTorrConfig", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("CacheSize")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ConnectionsLimit")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("DisableDHT")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("DisablePEX")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("DisableTCP")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("DisableUPNP")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("DisableUTP")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("DisableUpload")
                        .HasColumnType("INTEGER");

                    b.Property<long>("DownloadRateLimit")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("EnableDLNA")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("EnableDebug")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("EnableIPv6")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("EnableRutorSearch")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("ForceEncrypt")
                        .HasColumnType("INTEGER");

                    b.Property<string>("FriendlyName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("IdChat")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("NameProfileBot")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("PeersListenPort")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PreloadCache")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ReaderReadAHead")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("RemoveCacheOnDrop")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("ResponsiveMode")
                        .HasColumnType("INTEGER");

                    b.Property<int>("RetrackersMode")
                        .HasColumnType("INTEGER");

                    b.Property<string>("SslCert")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("SslKey")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("SslPort")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TorrentDisconnectTimeout")
                        .HasColumnType("INTEGER");

                    b.Property<string>("TorrentsSavePath")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<long>("UploadRateLimit")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("UseDisk")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("IdChat", "NameProfileBot")
                        .IsUnique();

                    b.ToTable("BitTorrConfig");
                });

            modelBuilder.Entity("AdTorrBot.BotTelegram.Db.Model.TorrserverModel.Profiles", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("AccessEndDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("AdminComment")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsEnabled")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Login")
                        .HasColumnType("TEXT");

                    b.Property<string>("Password")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("UniqueId")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("Login")
                        .IsUnique();

                    b.ToTable("Profiles");
                });

            modelBuilder.Entity("AdTorrBot.BotTelegram.Db.Model.TorrserverModel.ServerArgsConfig", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("DontKill")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Help")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("HttpAuth")
                        .HasColumnType("INTEGER");

                    b.Property<string>("IdChat")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("LogPath")
                        .HasColumnType("TEXT");

                    b.Property<string>("NameProfileBot")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Path")
                        .HasColumnType("TEXT");

                    b.Property<int?>("Port")
                        .HasColumnType("INTEGER");

                    b.Property<string>("PubIPv4")
                        .HasColumnType("TEXT");

                    b.Property<string>("PubIPv6")
                        .HasColumnType("TEXT");

                    b.Property<bool>("ReadOnlyMode")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("SearchWa")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Ssl")
                        .HasColumnType("INTEGER");

                    b.Property<string>("SslCert")
                        .HasColumnType("TEXT");

                    b.Property<string>("SslKey")
                        .HasColumnType("TEXT");

                    b.Property<int?>("SslPort")
                        .HasColumnType("INTEGER");

                    b.Property<string>("TorrentAddr")
                        .HasColumnType("TEXT");

                    b.Property<string>("TorrentsDir")
                        .HasColumnType("TEXT");

                    b.Property<bool>("Ui")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Version")
                        .HasColumnType("INTEGER");

                    b.Property<string>("WebLogPath")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("ServerArgsConfig");
                });

            modelBuilder.Entity("AdTorrBot.BotTelegram.Db.Model.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("IdChat")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("User");
                });
#pragma warning restore 612, 618
        }
    }
}
