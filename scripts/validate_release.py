import argparse
import re
import subprocess
from typing import TypedDict


class Args(TypedDict):
    tag: str


def main():
    args = get_args()

    version = args["tag"].lstrip("v")

    validate_changelog(version)
    validate_code(version)

    changelog_current_version_content = get_changelog_current_version_content(version)

    print("## What's Changed:")
    print(changelog_current_version_content)


def get_args() -> Args:
    parser = argparse.ArgumentParser()
    parser.add_argument("--tag", required=True)

    args = parser.parse_args()

    return {
        "tag": args.tag,
    }


def get_changelog_current_version_content(version: str) -> str:
    with open("CHANGELOG.md", "r", encoding="utf-8") as f:
        content = f.read()

    regex = r"## \[{}\] - .*?\n(.*?)## \[".format(version)

    result = re.search(regex, content, re.DOTALL)

    if not result:
        raise Exception("CHANGELOG.md lacks version {}".format(version))

    return result.group(1)


def validate_changelog(version: str):
    try:
        subprocess.run(
            f"npx changelog --format markdownlint",
            shell=True,
            check=True,
        )
    except subprocess.CalledProcessError as e:
        print("Have you installed it by `npm i -g keep-a-changelog`?")
        raise e

    with open("CHANGELOG.md", "r", encoding="utf-8") as f:
        content = f.read()

    if not re.search(r"## \[{}\]".format(version), content):
        raise Exception("CHANGELOG.md lacks version {}".format(version))


def validate_code(version: str):
    with open("cmd/lip/main.go", "r", encoding="utf-8") as f:
        content = f.read()

    if not re.search(r'semver\.MustParse\("{}"\)'.format(version), content):
        raise Exception("cmd/lip/main.go lacks version {}".format(version))

if __name__ == "__main__":
    main()
