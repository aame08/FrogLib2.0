using Microsoft.EntityFrameworkCore;

namespace FrogLib.Server.Models;

public partial class Froglib2Context : DbContext
{
    public Froglib2Context()
    {
    }

    public Froglib2Context(DbContextOptions<Froglib2Context> options)
        : base(options)
    {
    }

    public virtual DbSet<Author> Authors { get; set; }

    public virtual DbSet<Book> Books { get; set; }

    public virtual DbSet<Bookrating> Bookratings { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Collection> Collections { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<Entityrating> Entityratings { get; set; }

    public virtual DbSet<Likedcollection> Likedcollections { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Publisher> Publishers { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Userbook> Userbooks { get; set; }

    public virtual DbSet<Viewhistory> Viewhistories { get; set; }

    public virtual DbSet<Violation> Violations { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var connectionString = Environment.GetEnvironmentVariable("DefaultConnection")
                                ?? "server=localhost;user=root;password=ame0372;database=froglib2";
            optionsBuilder
                .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
                .UseLazyLoadingProxies();
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Author>(entity =>
        {
            entity.HasKey(e => e.IdAuthor).HasName("PRIMARY");

            entity.ToTable("authors");

            entity.Property(e => e.IdAuthor).HasColumnName("id_author");
            entity.Property(e => e.NameAuthor)
                .HasMaxLength(100)
                .HasColumnName("name_author");
            entity.Property(e => e.PatronymicAuthor)
                .HasMaxLength(100)
                .HasColumnName("patronymic_author");
            entity.Property(e => e.SurnameAuthor)
                .HasMaxLength(100)
                .HasColumnName("surname_author");

            entity.HasMany(d => d.IdBooks).WithMany(p => p.IdAuthors)
                .UsingEntity<Dictionary<string, object>>(
                    "Bookauthor",
                    r => r.HasOne<Book>().WithMany()
                        .HasForeignKey("IdBook")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("bookauthors_ibfk_2"),
                    l => l.HasOne<Author>().WithMany()
                        .HasForeignKey("IdAuthor")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("bookauthors_ibfk_1"),
                    j =>
                    {
                        j.HasKey("IdAuthor", "IdBook")
                            .HasName("PRIMARY")
                            .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
                        j.ToTable("bookauthors");
                        j.HasIndex(new[] { "IdBook" }, "id_book");
                        j.IndexerProperty<int>("IdAuthor").HasColumnName("id_author");
                        j.IndexerProperty<int>("IdBook").HasColumnName("id_book");
                    });
        });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.IdBook).HasName("PRIMARY");

            entity.ToTable("books");

            entity.HasIndex(e => e.IdCategory, "id_category");

            entity.HasIndex(e => e.IdPublisher, "id_publisher");

            entity.HasIndex(e => e.Isbn13, "isbn_13").IsUnique();

            entity.Property(e => e.IdBook).HasColumnName("id_book");
            entity.Property(e => e.AddedDate).HasColumnName("added_date");
            entity.Property(e => e.DescriptionBook)
                .HasColumnType("text")
                .HasColumnName("description_book");
            entity.Property(e => e.IdCategory).HasColumnName("id_category");
            entity.Property(e => e.IdPublisher).HasColumnName("id_publisher");
            entity.Property(e => e.ImageUrl)
                .HasColumnType("text")
                .HasColumnName("image_url");
            entity.Property(e => e.Isbn13)
                .HasMaxLength(13)
                .IsFixedLength()
                .HasColumnName("isbn_13");
            entity.Property(e => e.LanguageBook)
                .HasMaxLength(10)
                .HasColumnName("language_book");
            entity.Property(e => e.PageCount).HasColumnName("page_count");
            entity.Property(e => e.PreviewLink)
                .HasColumnType("text")
                .HasColumnName("preview_link");
            entity.Property(e => e.TitleBook)
                .HasMaxLength(255)
                .HasColumnName("title_book");
            entity.Property(e => e.YearPublication)
                .HasColumnType("year")
                .HasColumnName("year_publication");

            entity.HasOne(d => d.IdCategoryNavigation).WithMany(p => p.Books)
                .HasForeignKey(d => d.IdCategory)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("books_ibfk_2");

            entity.HasOne(d => d.IdPublisherNavigation).WithMany(p => p.Books)
                .HasForeignKey(d => d.IdPublisher)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("books_ibfk_1");
        });

        modelBuilder.Entity<Bookrating>(entity =>
        {
            entity.HasKey(e => e.IdRating).HasName("PRIMARY");

            entity.ToTable("bookratings");

            entity.HasIndex(e => e.IdBook, "id_book");

            entity.HasIndex(e => new { e.IdUser, e.IdBook }, "id_user").IsUnique();

            entity.Property(e => e.IdRating).HasColumnName("id_rating");
            entity.Property(e => e.IdBook).HasColumnName("id_book");
            entity.Property(e => e.IdUser).HasColumnName("id_user");
            entity.Property(e => e.Rating).HasColumnName("rating");

            entity.HasOne(d => d.IdBookNavigation).WithMany(p => p.Bookratings)
                .HasForeignKey(d => d.IdBook)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("bookratings_ibfk_2");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.Bookratings)
                .HasForeignKey(d => d.IdUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("bookratings_ibfk_1");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.IdCategory).HasName("PRIMARY");

            entity.ToTable("categories");

            entity.HasIndex(e => e.NameCategoryEng, "name_category_eng").IsUnique();

            entity.HasIndex(e => e.NameCategoryRu, "name_category_ru").IsUnique();

            entity.Property(e => e.IdCategory).HasColumnName("id_category");
            entity.Property(e => e.NameCategoryEng)
                .HasMaxLength(200)
                .HasColumnName("name_category_eng");
            entity.Property(e => e.NameCategoryRu)
                .HasMaxLength(200)
                .HasColumnName("name_category_ru");
        });

        modelBuilder.Entity<Collection>(entity =>
        {
            entity.HasKey(e => e.IdCollection).HasName("PRIMARY");

            entity.ToTable("collections");

            entity.HasIndex(e => e.IdUser, "id_user");

            entity.Property(e => e.IdCollection).HasColumnName("id_collection");
            entity.Property(e => e.CreatedDate).HasColumnName("created_date");
            entity.Property(e => e.DescriptionCollection)
                .HasColumnType("text")
                .HasColumnName("description_collection");
            entity.Property(e => e.IdUser).HasColumnName("id_user");
            entity.Property(e => e.StatusCollection)
                .HasColumnType("enum('На рассмотрении','Обнаружено нарушение','Одобрено','Отказано')")
                .HasColumnName("status_collection");
            entity.Property(e => e.TitleCollection)
                .HasMaxLength(255)
                .HasColumnName("title_collection");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.Collections)
                .HasForeignKey(d => d.IdUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("collections_ibfk_1");

            entity.HasMany(d => d.IdBooks).WithMany(p => p.IdCollections)
                .UsingEntity<Dictionary<string, object>>(
                    "Bookcollection",
                    r => r.HasOne<Book>().WithMany()
                        .HasForeignKey("IdBook")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("bookcollections_ibfk_2"),
                    l => l.HasOne<Collection>().WithMany()
                        .HasForeignKey("IdCollection")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("bookcollections_ibfk_1"),
                    j =>
                    {
                        j.HasKey("IdCollection", "IdBook")
                            .HasName("PRIMARY")
                            .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
                        j.ToTable("bookcollections");
                        j.HasIndex(new[] { "IdBook" }, "id_book");
                        j.IndexerProperty<int>("IdCollection").HasColumnName("id_collection");
                        j.IndexerProperty<int>("IdBook").HasColumnName("id_book");
                    });
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.IdComment).HasName("PRIMARY");

            entity.ToTable("comments");

            entity.HasIndex(e => e.IdUser, "id_user");

            entity.HasIndex(e => e.ParentCommentId, "parent_comment_id");

            entity.Property(e => e.IdComment).HasColumnName("id_comment");
            entity.Property(e => e.DateComment)
                .HasColumnType("datetime")
                .HasColumnName("date_comment");
            entity.Property(e => e.IdEntity).HasColumnName("id_entity");
            entity.Property(e => e.IdUser).HasColumnName("id_user");
            entity.Property(e => e.ParentCommentId).HasColumnName("parent_comment_id");
            entity.Property(e => e.StatusComment)
                .HasColumnType("enum('Обнаружено нарушение','Новое','Просмотрено')")
                .HasColumnName("status_comment");
            entity.Property(e => e.TextComment)
                .HasColumnType("text")
                .HasColumnName("text_comment");
            entity.Property(e => e.TypeEntity)
                .HasColumnType("enum('Рецензия','Подборка')")
                .HasColumnName("type_entity");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.Comments)
                .HasForeignKey(d => d.IdUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("comments_ibfk_1");

            entity.HasOne(d => d.ParentComment).WithMany(p => p.InverseParentComment)
                .HasForeignKey(d => d.ParentCommentId)
                .HasConstraintName("comments_ibfk_2");
        });

        modelBuilder.Entity<Entityrating>(entity =>
        {
            entity.HasKey(e => e.IdRating).HasName("PRIMARY");

            entity.ToTable("entityratings");

            entity.HasIndex(e => new { e.IdUser, e.IdEntity, e.TypeEntity }, "id_user").IsUnique();

            entity.Property(e => e.IdRating).HasColumnName("id_rating");
            entity.Property(e => e.IdEntity).HasColumnName("id_entity");
            entity.Property(e => e.IdUser).HasColumnName("id_user");
            entity.Property(e => e.IsLike).HasColumnName("is_like");
            entity.Property(e => e.TypeEntity)
                .HasColumnType("enum('Рецензия','Подборка','Комментарий')")
                .HasColumnName("type_entity");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.Entityratings)
                .HasForeignKey(d => d.IdUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("entityratings_ibfk_1");
        });

        modelBuilder.Entity<Likedcollection>(entity =>
        {
            entity.HasKey(e => new { e.IdCollection, e.IdUser })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.ToTable("likedcollections");

            entity.HasIndex(e => e.IdUser, "id_user");

            entity.Property(e => e.IdCollection).HasColumnName("id_collection");
            entity.Property(e => e.IdUser).HasColumnName("id_user");
            entity.Property(e => e.LikedDate).HasColumnName("liked_date");

            entity.HasOne(d => d.IdCollectionNavigation).WithMany(p => p.Likedcollections)
                .HasForeignKey(d => d.IdCollection)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("likedcollections_ibfk_1");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.Likedcollections)
                .HasForeignKey(d => d.IdUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("likedcollections_ibfk_2");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.IdNotification).HasName("PRIMARY");

            entity.ToTable("notifications");

            entity.HasIndex(e => e.IdRecipient, "id_recipient");

            entity.Property(e => e.IdNotification).HasColumnName("id_notification");
            entity.Property(e => e.CreatedDate).HasColumnName("created_date");
            entity.Property(e => e.EntityId).HasColumnName("entity_id");
            entity.Property(e => e.EntityType)
                .HasColumnType("enum('Комментарий','Рецензия','Подборка')")
                .HasColumnName("entity_type");
            entity.Property(e => e.IdRecipient).HasColumnName("id_recipient");
            entity.Property(e => e.IsRead).HasColumnName("is_read");
            entity.Property(e => e.MessageNotification)
                .HasColumnType("text")
                .HasColumnName("message_notification");

            entity.HasOne(d => d.IdRecipientNavigation).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.IdRecipient)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("notifications_ibfk_1");
        });

        modelBuilder.Entity<Publisher>(entity =>
        {
            entity.HasKey(e => e.IdPublisher).HasName("PRIMARY");

            entity.ToTable("publishers");

            entity.HasIndex(e => e.NamePublisher, "name_publisher").IsUnique();

            entity.Property(e => e.IdPublisher).HasColumnName("id_publisher");
            entity.Property(e => e.NamePublisher)
                .HasMaxLength(100)
                .HasColumnName("name_publisher");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.IdReview).HasName("PRIMARY");

            entity.ToTable("reviews");

            entity.HasIndex(e => e.IdBook, "id_book");

            entity.HasIndex(e => e.IdUser, "id_user");

            entity.Property(e => e.IdReview).HasColumnName("id_review");
            entity.Property(e => e.CreatedDate).HasColumnName("created_date");
            entity.Property(e => e.IdBook).HasColumnName("id_book");
            entity.Property(e => e.IdUser).HasColumnName("id_user");
            entity.Property(e => e.StatusReview)
                .HasColumnType("enum('На рассмотрении','Обнаружено нарушение','Одобрено','Отказано')")
                .HasColumnName("status_review");
            entity.Property(e => e.TextReview)
                .HasColumnType("text")
                .HasColumnName("text_review");
            entity.Property(e => e.TitleReview)
                .HasMaxLength(255)
                .HasColumnName("title_review");

            entity.HasOne(d => d.IdBookNavigation).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.IdBook)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("reviews_ibfk_2");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.IdUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("reviews_ibfk_1");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.IdUser).HasName("PRIMARY");

            entity.ToTable("users");

            entity.HasIndex(e => e.LoginUser, "login_user").IsUnique();

            entity.HasIndex(e => e.NameUser, "name_user").IsUnique();

            entity.Property(e => e.IdUser).HasColumnName("id_user");
            entity.Property(e => e.LoginUser)
                .HasMaxLength(100)
                .HasColumnName("login_user");
            entity.Property(e => e.NameRole)
                .HasDefaultValueSql("'Пользователь'")
                .HasColumnType("enum('Пользователь','Модератор','Администратор')")
                .HasColumnName("name_role");
            entity.Property(e => e.NameUser).HasColumnName("name_user");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(60)
                .IsFixedLength()
                .HasColumnName("password_hash");
            entity.Property(e => e.ProfileImageUrl)
                .HasMaxLength(255)
                .HasColumnName("profile_image_url");
            entity.Property(e => e.RegistrationDate).HasColumnName("registration_date");
            entity.Property(e => e.StatusUser)
                .HasDefaultValueSql("'Активен'")
                .HasColumnType("enum('Активен','Заблокирован')")
                .HasColumnName("status_user");
        });

        modelBuilder.Entity<Userbook>(entity =>
        {
            entity.HasKey(e => new { e.IdUser, e.IdBook })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.ToTable("userbooks");

            entity.HasIndex(e => e.IdBook, "id_book");

            entity.Property(e => e.IdUser).HasColumnName("id_user");
            entity.Property(e => e.IdBook).HasColumnName("id_book");
            entity.Property(e => e.AddedDate).HasColumnName("added_date");
            entity.Property(e => e.ListType)
                .HasColumnType("enum('Читаю','В планах','Брошено','Прочитано','Любимые')")
                .HasColumnName("list_type");

            entity.HasOne(d => d.IdBookNavigation).WithMany(p => p.Userbooks)
                .HasForeignKey(d => d.IdBook)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("userbooks_ibfk_2");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.Userbooks)
                .HasForeignKey(d => d.IdUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("userbooks_ibfk_1");
        });

        modelBuilder.Entity<Viewhistory>(entity =>
        {
            entity.HasKey(e => new { e.IdUser, e.IdEntity, e.TypeEntity })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0, 0 });

            entity.ToTable("viewhistory");

            entity.Property(e => e.IdUser).HasColumnName("id_user");
            entity.Property(e => e.IdEntity).HasColumnName("id_entity");
            entity.Property(e => e.TypeEntity)
                .HasColumnType("enum('Книга','Рецензия','Подборка')")
                .HasColumnName("type_entity");
            entity.Property(e => e.ViewDate).HasColumnName("view_date");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.Viewhistories)
                .HasForeignKey(d => d.IdUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("viewhistory_ibfk_1");
        });

        modelBuilder.Entity<Violation>(entity =>
        {
            entity.HasKey(e => e.IdViolation).HasName("PRIMARY");

            entity.ToTable("violations");

            entity.HasIndex(e => e.IdUser, "id_user");

            entity.Property(e => e.IdViolation).HasColumnName("id_violation");
            entity.Property(e => e.CategoryViolation)
                .HasColumnType("enum('Спам','Оскорбления','Мошенничество','Призывы к насилию','Другое')")
                .HasColumnName("category_violation");
            entity.Property(e => e.DateViolation).HasColumnName("date_violation");
            entity.Property(e => e.DescriptionViolation)
                .HasColumnType("text")
                .HasColumnName("description_violation");
            entity.Property(e => e.IdUser).HasColumnName("id_user");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.Violations)
                .HasForeignKey(d => d.IdUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("violations_ibfk_1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
