# KSDG ASP.NET MVC Workshop #1 #

## 簡介 ##

這是一個簡單的 MVC 留言板程式，它有下列功能：

1. 留言與回覆留言。
2. 編輯與刪除留言與回覆。
3. 整合Facebook登入。
4. 切換主版。
5. 在 Validate Request = true 的情況下允許 HTML 內容。
6. 使用 Bootstrap, Bootsnipp 以及 Fontawesome 簡單美化。

## 環境與前置需求 ##

- Visual Studio 2013 Community Edition, Professional or Ultimate Edition
- SQL Server Express LocalDb
- 網路環境

## Steps ##

### 建立 MVC 專案 ###

1. 建立一個空白的 MVC 專案 (ASP.NET 空白專案，勾選 MVC 作為核心參考)


### 建立資料庫 ###

1. 在 App_Data 資料夾中新增一個空白資料庫 (這會需要 SQL Server Express LocalDb，但若自己有 SQL Server Express 或 SQL Server 也能用，但方法會不同)。
2. 在資料庫內新增 GuestBooks 表格，其指令碼為：

        CREATE TABLE [dbo].[GuestBooks] (
            [Id]  UNIQUEIDENTIFIER NOT NULL,
            [IdReply] UNIQUEIDENTIFIER NULL,
            [Name] NVARCHAR (250)   NOT NULL,
            [Email]   VARCHAR (500)NOT NULL,
            [Subject] NVARCHAR (250)   NOT NULL,
            [Body] NVARCHAR (4000)  NOT NULL,
            [DateCreated] DATETIME DEFAULT (getdate()) NOT NULL,
            PRIMARY KEY CLUSTERED ([Id] ASC)
        );


### 建立 Model ###

1. 新增 ADO.NET 實體資料模型，使用由資料庫產生的Code-First模型。
2. 連到剛才新增的資料庫，勾選 GuestBooks 表格，並勾選物件單數化或複數化。
3. 完成精靈，產生 DbContext 與 GuestBook 類別。

### 建立 Controller ###

1. 新增 HomeController 於 Controllers 資料夾。


### 建立主版頁 ###

1. 在 Views 資料夾內新增一個 Shared 資料夾，並新增兩個版型配置頁 (Layout Page)，分別是 _MasterPage.cshtml 與 _MasterPage2.cshtml。
2. 由 NuGet 安裝 Bootstrap 以及 Fontawesome 套件。
3. 修改 _MasterPage.cshtml 加入 Bootstrap 版型，並加入 PageCSS 以及 PageScripts 兩個 section。

        <!DOCTYPE html>

        <html>
        <head>
            <meta name="viewport" content="width=device-width" />
            <title>@ViewBag.Title</title>
            <link href="~/Content/bootstrap.min.css" rel="stylesheet" />
            <link href="~/Content/bootstrap-theme.min.css" rel="stylesheet" />
            <link href="~/Content/font-awesome.min.css" rel="stylesheet" />
            <link href="~/Content/MasterPage1.css" rel="stylesheet" />
            @RenderSection("PageCSS", false);
        </head>
        <body>
            <nav class="navbar navbar-inverse navbar-fixed-top" role="navigation">
                <div class="container">
                    <div class="navbar-header">
                        <button type="button" class="navbar-toggle" data-toggle="collapse" data-target="#bs-example-navbar-collapse-1">
                            <span class="sr-only">Toggle navigation</span>
                            <span class="icon-bar"></span>
                            <span class="icon-bar"></span>
                            <span class="icon-bar"></span>
                        </button>
                        <a class="navbar-brand" href="#">Guest Book MVC</a>
                    </div>
                    <div class="collapse navbar-collapse" id="bs-example-navbar-collapse-1">
                        <ul class="nav navbar-nav">
                            <li>
                                <a href="#">About</a>
                            </li>
                            <li>
                                <a href="#">Services</a>
                            </li>
                            <li>
                                <a href="#">Contact</a>
                            </li>
                        </ul>
                    </div>
                </div>
            </nav>

            <div class="container">
                @RenderBody()
            </div>

            <script src="~/Scripts/jquery-1.9.1.min.js"></script>
            <script src="~/Scripts/bootstrap.min.js"></script>
            @RenderSection("PageScripts", false);
        </body>
        </html>

之後的 View 新增，除了部份檢視頁 (Partial View) 或有明確指示外，均以 _MasterPage.cshtml 作主版型。

### 建立列表頁 ###

1. 在 HomeController 中新增 Index 方法的 View，利用 Scaffolding 產生，Model 為 GuestBook Model。
2. 在 Index 方法中加入下列程式。

        public ActionResult Index()
        {
            using (WorkshopDb db = new WorkshopDb())
            {
                query = db.GuestBooks;
                return View(query.ToList());
            }
        }
 
3. 測試 Index 頁面，但當下沒有資料。

### 建立新增表單 ###

1. 在 HomeController 中新增 Create 方法，以及其 View，一樣用 Scaffolding 產生。
2. 在 Create 方法中加入下列程式碼。

        public ActionResult Create(Guid? id = null)
        {
            if (id == null)
                ViewBag.PostId = "";
            else
                ViewBag.PostId = id.Value.ToString();

            return View();
        }

4. 建立另一個 Create 方法，處理來自表單的 POST。

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(GuestBook Model, Guid? id = null)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var Model = new GuestBook();

            // create data.
            using (WorkshopDb db = new WorkshopDb())
            {
                if (id == null)
                {
                    Model.Id = Guid.NewGuid();
                    Model.DateCreated = DateTime.Now;

                    db.GuestBooks.Add(Model);
                }
                else
                {
                    Model.Id = Guid.NewGuid();
                    Model.IdReply = id.Value;
                    Model.DateCreated = DateTime.Now;

                    db.GuestBooks.Add(Model);
                }

                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

3. 測試 Create，應可以正常新增資料。

### 建立明細頁 (可瀏覽主題回覆) ###

1. 修改 Index 方法，讓它可以接受 ID。

        public ActionResult Index(Guid? id)
        {
            using (WorkshopDb db = new WorkshopDb())
            {
                IQueryable<GuestBook> query = null;

                if (id == null)
                {
                    query = db.GuestBooks.Where(i => i.IdReply == Guid.Empty);
                    return View("Thread", query.ToList());
                }
                else
                {
                    query = db.GuestBooks.Where(i => i.Id == id.Value);
                    ViewBag.PostId = id.Value.ToString();

                    if (query.Any())
                        return View("ThreadView", query.First());
                    else
                        return HttpNotFound();
                }
            }
        }

2. 修改Index.cshtml的名稱為Thread.cshtml，並新增一個View名為ThreadView.cshtml (空白View即可)。
3. 

### 建立修改表單 ###


### 刪除功能 ###


### 分頁能力 ###


### Facebook 整合 ###


### 切換 Master Page (Layout) ###


### 美化表單 ###


### 上傳到 GitHub ###