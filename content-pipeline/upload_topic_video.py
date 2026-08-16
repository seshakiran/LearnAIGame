#!/usr/bin/env python3
"""Upload one approved LearnAIGame topic MP4 to its versioned S3 media key.

The script intentionally uploads only a finished local file. Human approval and
playback review remain prerequisite release steps; this command does not publish
or alter any Unity content manifest.
"""

from __future__ import annotations

import argparse
import json
import mimetypes
import os
import re
import sys
from datetime import UTC, datetime
from pathlib import Path
from urllib.parse import quote

import boto3
from botocore.exceptions import BotoCoreError, ClientError

TOPIC_ID_PATTERN = re.compile(r"^[a-z][a-z0-9_]{1,62}$")
DEFAULT_CACHE_CONTROL = "public, max-age=31536000, immutable"


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Upload one approved topic video to videos/<topicId>/<version>.mp4."
    )
    parser.add_argument("local_video", type=Path, help="Path to the approved local .mp4 file.")
    parser.add_argument("topic_id", help="Topic ID, matching the card-burst topicId exactly.")
    parser.add_argument(
        "--bucket",
        default=os.getenv("S3_VIDEO_BUCKET"),
        help="Destination S3 bucket. Defaults to S3_VIDEO_BUCKET.",
    )
    parser.add_argument(
        "--region",
        default=os.getenv("AWS_REGION") or os.getenv("AWS_DEFAULT_REGION") or "us-east-1",
        help="AWS region for the destination bucket. Defaults to AWS_REGION or us-east-1.",
    )
    parser.add_argument(
        "--version",
        default=datetime.now(UTC).strftime("v%Y%m%dt%H%M%sz"),
        help="Immutable release label. Defaults to a UTC timestamp, e.g. v20260816t174500z.",
    )
    parser.add_argument(
        "--stream-base-url",
        default=os.getenv("S3_VIDEO_BASE_URL"),
        help=(
            "Public media base URL, normally a CDN domain such as "
            "https://media.example.com. Defaults to S3_VIDEO_BASE_URL. "
            "If omitted, the script prints the direct S3 HTTPS endpoint."
        ),
    )
    return parser.parse_args()


def validate(args: argparse.Namespace) -> None:
    if not args.bucket:
        raise ValueError("A destination bucket is required: pass --bucket or set S3_VIDEO_BUCKET.")
    if not TOPIC_ID_PATTERN.fullmatch(args.topic_id):
        raise ValueError(
            "topic_id must be snake_case, start with a lowercase letter, and contain only lowercase letters, digits, or underscores."
        )
    if not re.fullmatch(r"v[a-z0-9][a-z0-9._-]{0,62}", args.version):
        raise ValueError(
            "version must start with 'v' and contain only lowercase letters, digits, periods, underscores, or hyphens."
        )
    if not args.local_video.is_file():
        raise ValueError(f"Video file does not exist: {args.local_video}")
    if args.local_video.suffix.lower() != ".mp4":
        raise ValueError("Only .mp4 source files are accepted for this pipeline.")
    if args.local_video.stat().st_size == 0:
        raise ValueError("Video file is empty.")


def build_stream_url(bucket: str, region: str, key: str, base_url: str | None) -> str:
    encoded_key = quote(key, safe="/")
    if base_url:
        return f"{base_url.rstrip('/')}/{encoded_key}"
    if region == "us-east-1":
        return f"https://{bucket}.s3.amazonaws.com/{encoded_key}"
    return f"https://{bucket}.s3.{region}.amazonaws.com/{encoded_key}"


def main() -> int:
    args = parse_args()
    try:
        validate(args)
    except ValueError as error:
        print(f"Input error: {error}", file=sys.stderr)
        return 2

    key = f"videos/{args.topic_id}/{args.version}.mp4"
    content_type, _ = mimetypes.guess_type(str(args.local_video))
    extra_args = {
        "ContentType": content_type or "video/mp4",
        "CacheControl": DEFAULT_CACHE_CONTROL,
        "Metadata": {
            "topic-id": args.topic_id,
            "release-version": args.version,
            "source-filename": args.local_video.name,
        },
    }

    try:
        s3 = boto3.client("s3", region_name=args.region)
        s3.upload_file(
            Filename=str(args.local_video),
            Bucket=args.bucket,
            Key=key,
            ExtraArgs=extra_args,
        )
    except (BotoCoreError, ClientError) as error:
        print(f"Upload failed: {error}", file=sys.stderr)
        return 1

    result = {
        "topicId": args.topic_id,
        "version": args.version,
        "s3Uri": f"s3://{args.bucket}/{key}",
        "streamUrl": build_stream_url(args.bucket, args.region, key, args.stream_base_url),
        "contentType": extra_args["ContentType"],
        "cacheControl": extra_args["CacheControl"],
    }
    print(json.dumps(result, indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
