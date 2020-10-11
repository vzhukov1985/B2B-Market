using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Internal_WebApi.Controllers
{
    [Route("images")]
    public class ImagesController : Controller
    {

        [HttpGet("{type}/{guid}")]
        public IActionResult Get(string type, string guid)
        {
            string imgDir, imgFileName, imgPath;
            imgFileName = guid + ".jpg";
            switch (type)
            {
                case "topcategory":
                    imgDir = $"/Users/v_zhukov/ftp/Pictures/TopCategories/";
                    break;
                case "supplier":
                    imgDir = $"/Users/v_zhukov/ftp/Pictures/Suppliers/";
                    break;
                default:
                    return BadRequest("asd");
            }

            imgPath = imgDir + imgFileName;
            if (!System.IO.File.Exists(imgPath))
            {
                imgPath = imgDir + "noimage.jpg";
            }
            return PhysicalFile(imgPath, "image/jpeg");
        }

        [HttpGet("product/{type}/{guid}")]
        public IActionResult GetProductImage(string type, string guid)
        {
            string imgDir, imgFileName, imgPath;
            imgFileName = guid + ".jpg";
            switch (type)
            {
                case "matched":
                    imgDir = $"/Users/v_zhukov/ftp/Pictures/Products/Matched/";
                    break;
                case "unmatched":
                    imgDir = $"/Users/v_zhukov/ftp/Pictures/Products/Unmatched/";
                    break;
                case "conflicted":
                    imgDir = $"/Users/v_zhukov/ftp/Pictures/Products/Conflicted/";
                    break;
                default:
                    return BadRequest();
            }

            imgPath = imgDir + imgFileName;
            if (!System.IO.File.Exists(imgPath))
            {
                imgPath = "/Users/v_zhukov/ftp/Pictures/Products/noimage.jpg";
            }
            return PhysicalFile(imgPath, "image/jpeg");

        }

        //        private IActionResult GetImage()
    }
}
