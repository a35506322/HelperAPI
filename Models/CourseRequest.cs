using FluentValidation;
using System;
using System.ComponentModel.DataAnnotations;

namespace HelperAPI.Models
{
    public record CourseRequest (string BookId,string BookName);
}
