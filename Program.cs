using NLog;
using System.Linq;
using System.ComponentModel.DataAnnotations;

// See https://aka.ms/new-console-template for more information
string path = Directory.GetCurrentDirectory() + "\\nlog.config";

// create instance of Logger
var logger = LogManager.LoadConfiguration(path).GetCurrentClassLogger();
logger.Info("Program started");

try
{
    string choice;
    do
    {
        Console.WriteLine("Enter your selection:");
        Console.WriteLine("1) Display all blogs");
        Console.WriteLine("2) Add Blog");
        Console.WriteLine("3) Create Post");
        Console.WriteLine("4) Display Posts");
        Console.WriteLine("5) Delete Blog");
        Console.WriteLine("6) Edit Blog");
        Console.WriteLine("Enter q to quit");
        choice = Console.ReadLine();
        Console.Clear();
        logger.Info("Option {choice} selected", choice);

        var db = new BloggingContext();

        if (choice == "1")
        {
            // display blogs
            var query = db.Blogs.OrderBy(b => b.Name);

            Console.WriteLine($"{query.Count()} Blogs returned");
            foreach (var item in query)
            {
                Console.WriteLine(item.Name);
            }
        }
        else if (choice == "2")
        {
            // Add blog
            Blog blog = InputBlog(db, logger);
            if (blog != null)
            {
                //blog.BlogId = BlogId;
                db.AddBlog(blog);
                logger.Info("Blog added - {name}", blog.Name);
            }
        }
                if (choice == "3")
        {
            // display blogs
            Console.WriteLine("Chose blog to add a post to:");
            var blog = GetBlog(db, logger);
            //Add Post
            Console.WriteLine("Enter Post Title");
            var Title = Console.ReadLine();
            Console.WriteLine("Enter Post Content");
            var Content = Console.ReadLine();


        }
                if (choice == "4")
        {
            // display blogs
            var query = db.Blogs.OrderBy(b => b.Name);
            foreach (var item in query)
            {
                Console.WriteLine(item.Name);
            }
        }
        else if (choice == "5")
        {
            // delete blog
            Console.WriteLine("Choose the blog to delete:");
            var blog = GetBlog(db, logger);
            if (blog != null)
            {
                // delete blog
                db.DeleteBlog(blog);
                logger.Info($"Blog (id: {blog.BlogId}) deleted");
            }
        }
        else if (choice == "6")
        {
            // edit blog
            Console.WriteLine("Choose the blog to edit:");
            var blog = GetBlog(db, logger);
            if (blog != null)
            {
                // input blog
                Blog UpdatedBlog = InputBlog(db, logger);
                if (UpdatedBlog != null)
                {
                    UpdatedBlog.BlogId = blog.BlogId;
                    db.EditBlog(UpdatedBlog);
                    logger.Info($"Blog (id: {blog.BlogId}) updated");
                }
            }
        }
        Console.WriteLine();
    } while (choice.ToLower() != "q");
}
catch (Exception ex)
{
    logger.Error(ex.Message);
}

logger.Info("Program ended");

static Blog GetBlog(BloggingContext db, Logger logger)
{
    // display all blogs
    var blogs = db.Blogs.OrderBy(b => b.BlogId);
    foreach (Blog b in blogs)
    {
        Console.WriteLine($"{b.BlogId}: {b.Name}");
    }
    if (int.TryParse(Console.ReadLine(), out int BlogId))
    {
        Blog blog = db.Blogs.FirstOrDefault(b => b.BlogId == BlogId);
        if (blog != null)
        {
            return blog;
        }
    }
    logger.Error("Invalid Blog Id");
    return null;
}

static Blog InputBlog(BloggingContext db, Logger logger)
{
    Blog blog = new Blog();
    Console.WriteLine("Enter the Blog name");
    blog.Name = Console.ReadLine();

    ValidationContext context = new ValidationContext(blog, null, null);
    List<ValidationResult> results = new List<ValidationResult>();

    var isValid = Validator.TryValidateObject(blog, context, results, true);
    if (isValid)
    {
        // prevent duplicate blog names
        if (db.Blogs.Any(b => b.Name == blog.Name)) {
            // generate error
             results.Add(new ValidationResult("Blog name exists", new string[] { "Name" }));
        } else {
            return blog;
        }
    }
    {
        foreach (var result in results)
        {
            logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
        }
    }
    return null;
}
