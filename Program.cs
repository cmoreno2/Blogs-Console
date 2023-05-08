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
            db = new BloggingContext();
            //create qurey to order the blogs 
            var query = db.Blogs.OrderBy(b => b.BlogId);
            //creating new post
            var post = new Post();
            // display blogs
            do
            {
                Console.WriteLine("Chose blog to add a post to:");
                foreach (var item in query)
                {
                    Console.WriteLine($"{item.BlogId}): {item.Name} ");
                }
                post.BlogId = Convert.ToInt32(Console.ReadLine());
                logger.Info("Option {BlogID} selected\n", post.BlogId);
                //Making sure user input is valid
                if(!query.Any(b => b.BlogId == post.BlogId))
                {
                    logger.Error("Must Enter Valid Blog ID");
                }
            } while (!query.Any(b => b.BlogId == post.BlogId));
            //Add Post
            //Post title
            do{
            //Post Title [Required]
            Console.WriteLine("Enter Post Title");
            post.Title = Console.ReadLine();
            //Post content (can be null)
            Console.WriteLine("Enter Post Content");
            post.Content = Console.ReadLine();
            //Making sure post title in not null
            if(string.IsNullOrEmpty(post.Title))
            {
                logger.Error("Must Enter Post Title");
            }
            }while(string.IsNullOrEmpty(post.Title));
            //saving post to db
            db.AddPost(post);
            logger.Info("Post Added");
        }
                if (choice == "4")
        {
            db = new BloggingContext();
            //create qurey to order the blogs 
            var blogQuery = db.Blogs.OrderBy(b => b.BlogId);
            int UserPostInput;
            do
            {
            // display blogs
            Console.WriteLine("Which blogs posts do you want to see:");
            Console.WriteLine("0): Display all posts");
                foreach (var item in blogQuery)
                {
                    Console.WriteLine($"{item.BlogId}): {item.Name}");
                }
                 UserPostInput = Convert.ToInt32(Console.ReadLine());
                logger.Info("Option {BlogID} selected", UserPostInput);
            } while (!blogQuery.Any(b => b.BlogId == UserPostInput || UserPostInput == 0));
            //display post
            if(UserPostInput == 0)
            {
            var postQuery = db.Posts.OrderBy(b => b.BlogId);
            Console.WriteLine($"There are {postQuery.Count()} post(s)");
            foreach (var item in postQuery)
            {
                Console.WriteLine($"Blog: {item.Blog.Name} \nPost Title: {item.Title} \nPost Content: {item.Content}\n");
            }
            }else
            {
            var postQuery = db.Posts.Where(p => p.BlogId == UserPostInput).OrderBy(p => p.Title);
            Console.WriteLine($"There are {postQuery.Count()} post(s) for this blog");
            foreach (var item in postQuery)
            {
                Console.WriteLine($"Blog: {item.Blog.Name} \nPost Title: {item.Title} \nPost Content: {item.Content}\n");
            }
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
