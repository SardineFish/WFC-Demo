# Utils for Unity

Some useful utility codes for Unity development maintained by myself from 2017 :)

## Recommand using with git-subtree

Add `sub-tree`
```shell
$ git remote add sar-utils git@github.com:SardineFish/unity-utils.git
$ git subtree add --prefix Assets/Scripts/Utils sar-utils master --squash
```

Update `sub-tree`
```shell
$ git subtree pull --prefix Assets/Scripts/Utils sar-utils master --suqsh
```

Contributing back upstream
```shell
$ git subtree push --prefix Assets/Scripts/Utils sar-utils master
```