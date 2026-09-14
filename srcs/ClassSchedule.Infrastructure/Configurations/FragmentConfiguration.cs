using ClassSchedule.Domain.Entities;
using ClassSchedule.Domain.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace ClassSchedule.Infrastructure.Configurations;

/// <summary>
/// <see cref="Fragment"/> 的实体配置
/// </summary>
internal static class FragmentConfiguration
{
    /// <summary>
    /// 配置课程片段
    /// </summary>
    /// <param name="builder">课程片段的关联构建器</param>
    internal static void Configure(OwnedNavigationBuilder<Course, Fragment> builder)
    {
        _ = builder.Property(f => f.Id).ValueGeneratedNever();
        _ = builder.Property(f => f.Period).HasConversion<OrdinalRangeConverter>();
        _ = builder
            .Property(f => f.Weeks)
            .HasConversion<OrdinalRangesConverter, OrdinalRangesComparer>();
    }

    /// <summary>
    /// <see cref="OrdinalRange"/> 与 JSON 字符串之间的转换器
    /// </summary>
    private sealed class OrdinalRangeConverter() : ValueConverter<OrdinalRange, string>(
        range => JsonSerializer.Serialize(range, FileSystem.JsonSerializerOptions),
        json => JsonSerializer.Deserialize<OrdinalRange>(json, FileSystem.JsonSerializerOptions)
    );

    /// <summary>
    /// <see cref="OrdinalRange"/> 列表与 JSON 字符串之间的转换器
    /// </summary>
    private sealed class OrdinalRangesConverter() : ValueConverter<IReadOnlyList<OrdinalRange>, string>(
        ranges => JsonSerializer.Serialize(ranges, FileSystem.JsonSerializerOptions),
        json => JsonSerializer.Deserialize<List<OrdinalRange>>(json, FileSystem.JsonSerializerOptions)!
    );

    /// <summary>
    /// <see cref="OrdinalRange"/> 列表的比较器
    /// </summary>
    private sealed class OrdinalRangesComparer() : ValueComparer<IReadOnlyList<OrdinalRange>>(
        (left, right) => left!.SequenceEqual(right!),
        ranges => ranges.Aggregate(0, static (hash, range) => HashCode.Combine(hash, range.GetHashCode())),
        ranges => ranges.ToList()
    );
}
