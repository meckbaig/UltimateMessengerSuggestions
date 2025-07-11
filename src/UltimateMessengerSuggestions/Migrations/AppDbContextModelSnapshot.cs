﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using UltimateMessengerSuggestions.DbContexts;

#nullable disable

namespace UltimateMessengerSuggestions.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("MediaFileTag", b =>
                {
                    b.Property<int>("MediaFilesId")
                        .HasColumnType("integer")
                        .HasColumnName("media_files_id");

                    b.Property<int>("TagsId")
                        .HasColumnType("integer")
                        .HasColumnName("tags_id");

                    b.HasKey("MediaFilesId", "TagsId")
                        .HasName("pk_media_file_tag");

                    b.HasIndex("TagsId")
                        .HasDatabaseName("ix_media_file_tag_tags_id");

                    b.ToTable("media_file_tag", (string)null);
                });

            modelBuilder.Entity("UltimateMessengerSuggestions.Models.Db.MediaFile", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("description");

                    b.Property<string>("MediaType")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("media_type");

                    b.Property<string>("MediaUrl")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("media_url");

                    b.HasKey("Id");

                    b.ToTable("media_files", null, t =>
                        {
                            t.HasCheckConstraint("CK_Message_Type", "\"media_type\" IN ('voice', 'picture')");
                        });

                    b.UseTptMappingStrategy();
                });

            modelBuilder.Entity("UltimateMessengerSuggestions.Models.Db.Tag", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.HasKey("Id")
                        .HasName("pk_tags");

                    b.HasIndex("Name")
                        .HasDatabaseName("ix_tags_name");

                    NpgsqlIndexBuilderExtensions.HasMethod(b.HasIndex("Name"), "gin");
                    NpgsqlIndexBuilderExtensions.HasOperators(b.HasIndex("Name"), new[] { "gin_trgm_ops" });

                    b.ToTable("tags", (string)null);
                });

            modelBuilder.Entity("UltimateMessengerSuggestions.Models.Db.VkVoiceMediaFile", b =>
                {
                    b.HasBaseType("UltimateMessengerSuggestions.Models.Db.MediaFile");

                    b.Property<string>("VkConversation")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("vk_conversation");

                    b.Property<long>("VkMessageId")
                        .HasColumnType("bigint")
                        .HasColumnName("vk_message_id");

                    b.ToTable("vk_voice_media_file", null, t =>
                        {
                            t.HasCheckConstraint("CK_Message_Type", "\"media_type\" IN ('voice', 'picture')");
                        });
                });

            modelBuilder.Entity("MediaFileTag", b =>
                {
                    b.HasOne("UltimateMessengerSuggestions.Models.Db.MediaFile", null)
                        .WithMany()
                        .HasForeignKey("MediaFilesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_media_file_tag_media_files_media_files_id");

                    b.HasOne("UltimateMessengerSuggestions.Models.Db.Tag", null)
                        .WithMany()
                        .HasForeignKey("TagsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_media_file_tag_tags_tags_id");
                });

            modelBuilder.Entity("UltimateMessengerSuggestions.Models.Db.VkVoiceMediaFile", b =>
                {
                    b.HasOne("UltimateMessengerSuggestions.Models.Db.MediaFile", null)
                        .WithOne()
                        .HasForeignKey("UltimateMessengerSuggestions.Models.Db.VkVoiceMediaFile", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_vk_voice_media_file_media_files_id");
                });
#pragma warning restore 612, 618
        }
    }
}
