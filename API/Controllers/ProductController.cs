using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API._Repositories.Interfaces;
using API._Services.Interfaces;
using API.Dtos;
using API.Helpers.Params;
using API.Helpers.Utilities;
using Aspose.Cells;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class ProductController : ApiController
    {
        private readonly IProductService _productService;
        private readonly IDropzoneService _dropzoneService;
        private readonly IProductRepository _productRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(
            IProductService productService,
            IWebHostEnvironment webHostEnvironment,
            IProductRepository productRepository,
            IDropzoneService dropzoneService)
        {
            _productService = productService;
            _webHostEnvironment = webHostEnvironment;
            _productRepository = productRepository;
            _dropzoneService = dropzoneService;
        }

        [HttpPost("create")]
        // [RequestSizeLimit(100000000)]
        public async Task<IActionResult> Create([FromForm] Product_Dto model)
        {
            model.FileImages = await _dropzoneService.UploadFile(model.Images, model.Product_Cate_ID + "_" + model.Product_ID + "_", "\\uploaded\\images\\product");
            model.FileVideos = await _dropzoneService.UploadFile(model.Videos, model.Product_Cate_ID + "_" + model.Product_ID + "_", "\\uploaded\\video\\product");
            model.Update_By = User.FindFirst(ClaimTypes.Name).Value;
            model.Update_Time = DateTime.Now;
            var data = await _productService.Create(model);
            return Ok(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetProductByID(string productCateID, int productID)
        {
            var data = await _productService.GetProductByID(productCateID, productID);
            return Ok(data);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllAsync()
        {
            var data = await _productService.GetAllAsync();
            return Ok(data);
        }

        [HttpGet("productID")]
        public async Task<IActionResult> GetListProductByProductCateID(string productCateID)
        {
            var data = await _productService.GetListProductByProductCateID(productCateID);
            return Ok(data);
        }

        [HttpGet("pagination")]
        public async Task<IActionResult> GetProductWithPaginations([FromQuery] PaginationParams param, string text)
        {
            var data = await _productService.GetProductWithPaginations(param, text);
            return Ok(data);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchProductWithPaginations([FromQuery] PaginationParams param, string productCateID, string productName)
        {
            var data = await _productService.SearchProductWithPaginations(param, productCateID, productName);
            return Ok(data);
        }

        [HttpPut]
        // [RequestSizeLimit(100000000)]
        public async Task<IActionResult> Update([FromForm] Product_Dto model)
        {
            if (model.Content == "null")
            {
                model.Content = null;
            }
            if (model.FileImages == "null")
            {
                model.FileImages = "";
            }
            if (model.FileVideos == "null")
            {
                model.FileVideos = "";
            }

            // Delete images, videos old
            var images = await _productRepository.FindAll(x => x.Product_Cate_ID == model.Product_Cate_ID &&
                                                    x.Product_ID == model.Product_ID &&
                                                    x.Product_Name == model.Product_Name).Select(x => x.FileImages).Distinct().FirstOrDefaultAsync();

            var videos = await _productRepository.FindAll(x => x.Product_Cate_ID == model.Product_Cate_ID &&
                                                    x.Product_ID == model.Product_ID &&
                                                    x.Product_Name == model.Product_Name).Select(x => x.FileVideos).Distinct().FirstOrDefaultAsync();
            if (!string.IsNullOrEmpty(images))
            {
                _dropzoneService.DeleteFileUpload(images, "\\uploaded\\images\\product");
            }
            if (!string.IsNullOrEmpty(videos))
            {
                _dropzoneService.DeleteFileUpload(videos, "\\uploaded\\video\\product");
            }

            // Add images, videos
            model.FileImages = null;
            model.FileVideos = null;

            model.FileImages = await _dropzoneService.UploadFile(model.Images, model.Product_Cate_ID + "_" + model.Product_ID + "_", "\\uploaded\\images\\product");
            model.FileVideos = await _dropzoneService.UploadFile(model.Videos, model.Product_Cate_ID + "_" + model.Product_ID + "_", "\\uploaded\\video\\product");

            model.Update_By = User.FindFirst(ClaimTypes.Name).Value;
            model.Update_Time = DateTime.Now;
            var data = await _productService.Update(model);
            return Ok(data);
        }

        [HttpPut("changeNew")]
        public async Task<IActionResult> ChangeNew(Product_Dto model)
        {
            model.Update_By = User.FindFirst(ClaimTypes.Name).Value;
            model.Update_Time = DateTime.Now;
            model.New = !model.New;
            var data = await _productService.Update(model);
            return Ok(data);
        }

        [HttpPut("changeHotSale")]
        public async Task<IActionResult> ChangeHotSale(Product_Dto model)
        {
            model.Update_By = User.FindFirst(ClaimTypes.Name).Value;
            model.Update_Time = DateTime.Now;
            model.Hot_Sale = !model.Hot_Sale;
            var data = await _productService.Update(model);
            return Ok(data);
        }

        [HttpPut("changeIsSale")]
        public async Task<IActionResult> ChangeIsSale(Product_Dto model)
        {
            model.Update_By = User.FindFirst(ClaimTypes.Name).Value;
            model.Update_Time = DateTime.Now;
            model.IsSale = !model.IsSale;
            if (model.IsSale == false)
            {
                model.Time_Sale = 0;
                model.From_Date_Sale = null;
                model.To_Date_Sale = null;
            }
            var data = await _productService.Update(model);
            return Ok(data);
        }

        [HttpPut("changeStatus")]
        public async Task<IActionResult> ChangeStatus(Product_Dto model)
        {
            model.Update_By = User.FindFirst(ClaimTypes.Name).Value;
            model.Update_Time = DateTime.Now;
            model.Status = !model.Status;
            var data = await _productService.Update(model);
            return Ok(data);
        }

        [HttpPost("delete")]
        public async Task<IActionResult> Remove(Product_Dto model)
        {
            var images = await _productRepository.FindAll(x => x.Product_Cate_ID == model.Product_Cate_ID &&
                                                    x.Product_ID == model.Product_ID &&
                                                    x.Product_Name == model.Product_Name).Select(x => x.FileImages).Distinct().FirstOrDefaultAsync();

            var videos = await _productRepository.FindAll(x => x.Product_Cate_ID == model.Product_Cate_ID &&
                                                    x.Product_ID == model.Product_ID &&
                                                    x.Product_Name == model.Product_Name).Select(x => x.FileVideos).Distinct().FirstOrDefaultAsync();
            if (!string.IsNullOrEmpty(images))
            {
                _dropzoneService.DeleteFileUpload(images, "\\uploaded\\images\\product");
            }
            if (!string.IsNullOrEmpty(videos))
            {
                _dropzoneService.DeleteFileUpload(videos, "\\uploaded\\video\\product");
            }
            var data = await _productService.Remove(model);
            return Ok(data);
        }

        // Export Excel and PDF Article with Aspose.Cell
        [HttpGet("exportExcelAspose")]
        public async Task<ActionResult> ExportExcelAspose([FromQuery] string productCateID, string productID, int changeExport)
        {
            var data = await _productService.GetProductByID(productCateID, productID.ToInt());
            var path = Path.Combine(_webHostEnvironment.ContentRootPath, "Resources\\Template\\Product\\ProductTemplate.xlsx");
            WorkbookDesigner designer = new WorkbookDesigner();
            designer.Workbook = new Workbook(path);

            Cell cell = designer.Workbook.Worksheets[0].Cells["A1"];
            Worksheet ws = designer.Workbook.Worksheets[0];
            Cells cells = ws.Cells;

            designer.SetDataSource("result", data);
            designer.Process();

            Style style = designer.Workbook.CreateStyle();
            style.IsTextWrapped = true;
            style.VerticalAlignment = TextAlignmentType.Center;
            style.HorizontalAlignment = TextAlignmentType.Center;
            style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
            style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
            style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
            style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;

            StyleFlag flg = new StyleFlag();
            flg.All = true;

            int index = 2;
            ws.Cells["A" + (index)].PutValue(data.Product_Cate_ID);
            ws.Cells["B" + (index)].PutValue(data.Product_Name);
            ws.Cells["C" + (index)].PutValue(data.New);
            ws.Cells["D" + (index)].PutValue(data.IsSale);
            ws.Cells["E" + (index)].PutValue(data.Hot_Sale);
            ws.Cells["F" + (index)].PutValue(data.Status);
            ws.Cells["G" + (index)].PutValue(data.Price);
            ws.Cells["H" + (index)].PutValue(data.Amount);
            ws.Cells["I" + (index)].PutValue(data.Update_By);
            ws.Cells["J" + (index)].PutValue(data.Update_Time.ToString());

            string folderImage = _webHostEnvironment.WebRootPath + "\\uploaded\\images\\product\\";
            string folderVideo = _webHostEnvironment.WebRootPath + "\\uploaded\\video\\product\\";
            if (data.FileImages != null)
            {
                string[] listImage = data.FileImages.Split(";");
                foreach (var image in listImage)
                {
                    if (image != "")
                    {
                        Aspose.Cells.Drawing.Picture pic = ws.Pictures[ws.Pictures.Add(index - 1, 10, folderImage + image)];
                        pic.Height = 40;
                        pic.Width = 80;
                        pic.Top = 10;
                        pic.Left = 20;

                        ws.Cells.SetRowHeight(index - 1, 45);

                        index++;
                    }
                }
            }
            int index1 = 2;
            if (data.FileVideos != null)
            {
                string[] listVideo = data.FileVideos.Split(";");
                foreach (var video in listVideo)
                {
                    if (video != "")
                    {
                        Aspose.Cells.Drawing.Picture pic = ws.Pictures[ws.Pictures.Add(index1 - 1, 11, folderVideo + "video.jpg")];
                        pic.Height = 40;
                        pic.Width = 80;
                        pic.Top = 10;
                        pic.Left = 20;

                        ws.Cells.SetRowHeight(index1 - 1, 45);

                        index1++;
                    }
                }
            }

            if (data.FileVideos != null || data.FileImages != null)
            {
                int index3 = 0;
                if (index > index1 || index == index1)
                    index3 = index;
                else
                    index3 = index1;

                cells.Merge(1, 0, index3 - 2, 1);
                cells.Merge(1, 1, index3 - 2, 1);
                cells.Merge(1, 2, index3 - 2, 1);
                cells.Merge(1, 3, index3 - 2, 1);
                cells.Merge(1, 4, index3 - 2, 1);
                cells.Merge(1, 5, index3 - 2, 1);
                cells.Merge(1, 6, index3 - 2, 1);
                cells.Merge(1, 7, index3 - 2, 1);
                cells.Merge(1, 8, index3 - 2, 1);
                cells.Merge(1, 9, index3 - 2, 1);

                for (int i = 1; i < index3 - 1; i++)
                {
                    ws.Cells["A" + (i + 1)].SetStyle(style, flg);
                    ws.Cells["B" + (i + 1)].SetStyle(style, flg);
                    ws.Cells["C" + (i + 1)].SetStyle(style, flg);
                    ws.Cells["D" + (i + 1)].SetStyle(style, flg);
                    ws.Cells["E" + (i + 1)].SetStyle(style, flg);
                    ws.Cells["F" + (i + 1)].SetStyle(style, flg);
                    ws.Cells["G" + (i + 1)].SetStyle(style, flg);
                    ws.Cells["H" + (i + 1)].SetStyle(style, flg);
                    ws.Cells["I" + (i + 1)].SetStyle(style, flg);
                    ws.Cells["J" + (i + 1)].SetStyle(style, flg);
                    ws.Cells["K" + (i + 1)].SetStyle(style, flg);
                    ws.Cells["L" + (i + 1)].SetStyle(style, flg);
                }
            }

            MemoryStream stream = new MemoryStream();

            string fileKind = "";
            string fileExtension = "";

            if (changeExport == 1)
            {
                designer.Workbook.Save(stream, SaveFormat.Xlsx);
                fileKind = "application/xlsx";
                fileExtension = ".xlsx";
            }
            if (changeExport == 2)
            {
                // custom size ( width: in, height: in )
                ws.PageSetup.FitToPagesTall = 0;
                ws.PageSetup.SetHeader(0, "&D &T");
                ws.PageSetup.SetHeader(1, "&B Article");
                ws.PageSetup.SetFooter(0, "&B SYSTEM BY MINH HIEU");
                ws.PageSetup.SetFooter(2, "&P/&N");
                ws.PageSetup.PrintQuality = 1200;
                designer.Workbook.Save(stream, SaveFormat.Pdf);
                fileKind = "application/pdf";
                fileExtension = ".pdf";
            }

            byte[] result = stream.ToArray();

            return File(result, fileKind, "Article_" + DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss") + fileExtension);
        }
    }
}