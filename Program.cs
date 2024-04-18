using NLog;
using System;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

public class Program
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    public static void Main(string[] args)
    {
        var path = Directory.GetCurrentDirectory() + "//nlog.config";
        LogManager.LoadConfiguration(path);
        logger.Info("Program started");

        using var db = new BloggingContext();

        try
        {
            bool running = true;
            while (running)
            {
                Console.WriteLine("\nChoose an option:");
                Console.WriteLine("1. Display all blogs");
                Console.WriteLine("2. Add a blog");
                Console.WriteLine("3. Create a post");
                Console.WriteLine("4. Display posts");
                Console.WriteLine("5. Exit");
                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        DisplayBlogs(db);
                        break;
                    case "2":
                        AddBlog(db);
                        break;
                    case "3":
                        CreatePost(db);
                        break;
                    case "4":
                        DisplayPosts(db);
                        break;
                    case "5":
                        running = false;
                        break;
                    default:
                        Console.WriteLine("Invalid option, try again.");
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            logger.Error(ex, "An exception occurred.");
        }

        logger.Info("Program ended");
    }

    private static void DisplayBlogs(BloggingContext db)
    {
        var blogs = db.Blogs.OrderBy(b => b.Name).ToList();
        Console.WriteLine($"Total blogs: {blogs.Count}");
        foreach (var blog in blogs)
        {
            Console.WriteLine($"Blog ID: {blog.BlogId}, Name: {blog.Name}");
        }
    }

    private static void AddBlog(BloggingContext db)
    {
        Console.Write("Enter a name for a new Blog: ");
        var name = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(name))
        {
            logger.Warn("Blog name cannot be empty.");
            Console.WriteLine("Blog name cannot be empty.");
            return;
        }
        var blog = new Blog { Name = name };
        db.Blogs.Add(blog);
        db.SaveChanges();
        logger.Info("Blog added - {0}", name);
        Console.WriteLine($"Blog '{name}' added.");
    }

    private static void CreatePost(BloggingContext db)
    {
        DisplayBlogs(db);
        Console.Write("Enter a Blog ID to post to: ");
        if (int.TryParse(Console.ReadLine(), out int blogId) && db.Blogs.Any(b => b.BlogId == blogId))
        {
            Console.Write("Enter the post title: ");
            var title = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(title))
            {
                logger.Warn("Post title cannot be empty.");
                Console.WriteLine("Post title cannot be empty.");
                return;
            }

            Console.Write("Enter the post content (optional): ");
            var content = Console.ReadLine();
            var post = new Post { Title = title, Content = content, BlogId = blogId };
            db.Posts.Add(post);
            db.SaveChanges();
            logger.Info("Post added - {0}", title);
            Console.WriteLine($"Post '{title}' added to blog ID {blogId}.");
        }
        else
        {
            logger.Warn("Invalid Blog ID.");
            Console.WriteLine("Invalid Blog ID.");
        }
    }

    private static void DisplayPosts(BloggingContext db)
    {
        Console.Write("Enter Blog ID to display posts (0 for all blogs): ");
        if (int.TryParse(Console.ReadLine(), out int blogId))
        {
            IQueryable<Post> postsQuery = db.Posts.Include(p => p.Blog);
            if (blogId != 0)
            {
                postsQuery = postsQuery.Where(p => p.BlogId == blogId);
            }

            var posts = postsQuery.ToList();
            if (posts.Any())
            {
                Console.WriteLine($"Total posts: {posts.Count}");
                foreach (var post in posts)
                {
                    Console.WriteLine($"Blog Name: {post.Blog.Name}, Post Title: {post.Title}, Post Content: {post.Content ?? "No content"}");
                }
            }
            else
            {
                Console.WriteLine("No posts found for this blog.");
            }
        }
        else
        {
            logger.Warn("Invalid input for Blog ID.");
            Console.WriteLine("Invalid input. Please enter a valid Blog ID.");
        }
    }
}

