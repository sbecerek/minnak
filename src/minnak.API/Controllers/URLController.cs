using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using LiteDB;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using minnak.Entities;

namespace minnak.Controllers
{

    [ApiController]
    [EnableCors("_myAllowSpecificOrigins")]
    [Route("api/URL/[Action]")]
    public class URLController : ControllerBase
    {
        private  ILiteDatabase _database{get;set;}

        private ILiteCollection<ShortLink> _collection{get;set;}

        public URLController(ILiteDatabase database)
        {
            _database = database;
            _collection = _database.GetCollection<ShortLink>("ShortLinks");
  
        }


        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<string>> Shorten([Required]string url, string? alias = null)
        {
            Response.Headers.Add("Access-Control-Allow-Origin", "*");

            if (!Uri.IsWellFormedUriString(url,UriKind.Absolute))
                return BadRequest("Invalid URL");

            var existingURL =_collection.Find(sl => sl.Url == url).FirstOrDefault();
            if(existingURL != null)
                return Ok($"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/{existingURL.Id}");

            var usedAlias = _collection.Find(sl=>sl.Id == alias).FirstOrDefault();
            if(usedAlias != null && !String.IsNullOrWhiteSpace(alias))
                return BadRequest("Alias is used under domain");


            ShortLink shortLink;
            if(String.IsNullOrWhiteSpace(alias))
                shortLink = new ShortLink(url);
            else{
                shortLink = new ShortLink{
                    Id = alias,
                    Url = url
                };
            }
            
            _collection.Insert(shortLink);
            
            var responseURI = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/{shortLink.Id}";

            return Ok(responseURI);
        }

        
    }
    
}