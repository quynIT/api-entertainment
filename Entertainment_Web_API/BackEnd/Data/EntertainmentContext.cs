using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using BackEnd.Models;
using Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Data;

public partial class EntertainmentContext : IdentityDbContext
{
    //Constructor
    public EntertainmentContext(DbContextOptions options) : base(options)
    {

    }

    public virtual DbSet<Video> Videos { get; set; }

    public virtual DbSet<AppUser> AppUser { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<Playlist> Playlist { get; set; }

    public virtual DbSet<VideoPlaylist> VideoPlaylists { get; set; }

    public virtual DbSet<ReplyComment> ReplyComments { get; set; }

    public virtual DbSet<UserVideoReaction> UserVideoReactions { get; set; }
    //    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
    //        => optionsBuilder.UseSqlServer("Data Source=LAPTOP-UTPMHK27\\SQLEXPRESS;Initial Catalog=Entertainment;Integrated Security=True;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<VideoPlaylist>(entity =>
        {
            entity.HasKey(e => new { e.VideoId, e.PlaylistId }).HasName("PK__VideoPla__0AC8567A7A214FA2");

            entity.ToTable("VideoPlaylist");

            entity.HasOne(d => d.Video).WithMany(p => p.VideoPlaylists)
                .HasForeignKey(d => d.VideoId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__VideoPlay__Video__6A30C649");

            entity.HasOne(d => d.Playlist).WithMany(p => p.VideoPlaylists)
                .HasForeignKey(d => d.PlaylistId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__VideoPlay__Playl__6B24EA82");
        });

        // Cấu hình cho phép xóa comment không bị ràng buộc bởi khóa ngoại của reply comment
        builder.Entity<ReplyComment>()
            .HasOne(r => r.Comment)
            .WithMany(c => c.Replies)
            .HasForeignKey(r => r.CommentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Cấu hình cho phép xóa tài khoản không bị ràng buộc bởi khóa ngoại của reply comment
        builder.Entity<AppUser>()
            .HasMany(u => u.Comments)
            .WithOne(c => c.AppUser)
            .HasForeignKey(c => c.Id)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<UserVideoReaction>(entity =>
        {
            entity.HasKey(e => new { e.VideoId, e.Id }).HasName("PK__UV__0AC8567A7A214FA3");

            entity.ToTable("UserVideoReaction");

            entity.HasOne(d => d.Video).WithMany(p => p.UserVideoReactions)
                .HasForeignKey(d => d.VideoId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__UV__Video__6A30C640");

            entity.HasOne(d => d.AppUser).WithMany(p => p.UserVideoReactions)
                .HasForeignKey(d => d.Id)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__UV__User__6B24EA83");
        });

        OnModelCreatingPartial(builder);
    }

    partial void OnModelCreatingPartial(ModelBuilder builder);
}