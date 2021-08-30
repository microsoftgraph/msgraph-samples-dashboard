// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useCallback, useEffect, useRef, useState } from 'react';
import { useMsal } from '@azure/msal-react';
import {
  DetailsListLayoutMode,
  IColumn,
  IStackProps,
  IStackStyles,
  IStackTokens,
  ITextFieldStyles,
  Link,
  ScrollablePane,
  ScrollbarVisibility,
  SelectionMode,
  ShimmeredDetailsList,
  Stack,
  Sticky,
  StickyPositionType,
  TextField,
} from '@fluentui/react';

import StatusCardRow from '../StatusCardRow/StatusCardRow';
import StatusIndicator from '../StatusIndicator/StatusIndicator';
import Owners from '../Owners/Owners';
import DateTime from '../DateTime/DateTime';
import GitHubLink from '../GitHubLink/GitHubLink';
import Repository from '../../types/Repository';
import { useAppContext } from '../../AppContext';
import { classNames, filterListClass } from '../SharedStyles';
import { repoListColumns } from '../../strings/Strings';
import { getRepositoryStats, UpdateStats } from '../../types/UpdateStats';
import { authConfig } from '../../AuthConfig';
import copyAndSort from '../../utilities/copy-and-sort';
import { repositoryColumns } from './RepositoryColumns';

interface RepositoriesProps {
  type: 'samples' | 'sdks';
}

// These constants are used to configure the stack
// layouts
const stackTokens: Partial<IStackTokens> = {
  childrenGap: 50,
};
const stackStyles: Partial<IStackStyles> = {
  root: { width: 650 },
};
const columnProps: Partial<IStackProps> = {
  tokens: { childrenGap: 15 },
  styles: { root: { width: 300 } },
};
const textFieldStyles: Partial<ITextFieldStyles> = {
  root: { maxWidth: '300px' },
};

// Used by the ShimmeredDetailList to render a single
// column within a row
function renderItemColumn(item: Repository, index?: number, column?: IColumn) {
  // Return the appropriate component based on the
  // column name
  switch (column?.name) {
    case repoListColumns.name:
      const name = item.name.toLowerCase();
      return (
        <Link href={`/repo/${name}`}>
          <span>{name}</span>
        </Link>
      );

    case repoListColumns.owners:
      return <Owners owners={item.owners} />;

    case repoListColumns.status:
      return (
        <StatusIndicator
          status={item.repositoryStatus}
          objectName='dependency'
          mode='repo'
          useUnknown={true}
        />
      );

    case repoListColumns.lastUpdated:
      return <DateTime dateTimeOffset={item.lastUpdated} />;

    case repoListColumns.identityLibs:
      return (
        <StatusIndicator
          status={item.identityStatus}
          objectName='Identity library'
          mode='repo'
          useUnknown={false}
        />
      );

    case repoListColumns.graphSdks:
      return (
        <StatusIndicator
          status={item.graphStatus}
          objectName='Graph SDK'
          mode='repo'
          useUnknown={false}
        />
      );

    case repoListColumns.securityAlerts:
      return (
        <GitHubLink
          repoUrl={item.url}
          relativeUrl='security/dependabot'
          text={item.securityAlerts.toString()}
          icon={item.securityAlerts > 0 ? 'WarningSolid' : undefined}
          iconClass={classNames.yellow}
        />
      );

    case repoListColumns.openPRs:
      return (
        <GitHubLink
          repoUrl={item.url}
          relativeUrl='pulls'
          text={item.pullRequests.toString()}
        />
      );

    case repoListColumns.openIssues:
      return (
        <GitHubLink
          repoUrl={item.url}
          relativeUrl='issues'
          text={item.issues.toString()}
        />
      );

    case repoListColumns.forks:
      return (
        <GitHubLink
          repoUrl={item.url}
          relativeUrl='network/members'
          text={item.forks.toString()}
        />
      );

    case repoListColumns.stars:
      return (
        <GitHubLink
          repoUrl={item.url}
          relativeUrl='stargazers'
          text={item.starGazers.toString()}
          icon='FavoriteStarFill'
          iconClass={classNames.yellow}
        />
      );

    case repoListColumns.views:
      return (
        <GitHubLink
          repoUrl={item.url}
          relativeUrl='graphs/traffic'
          text={item.views.toString()}
        />
      );

    case repoListColumns.language:
      return <span>{item.language}</span>;

    case repoListColumns.featureArea:
      return <span>{item.featureArea}</span>;
  }
}

export default function Repositories(props: RepositoriesProps) {
  // This holds all of the repositories returned by the
  // service. This is separate from the repositories constant
  // because repositories is potentially a filtered copy of
  // the full set
  const allRepositories = useRef<Repository[]>([]);
  // This holds the current view (could be filtered, sorted, etc.)
  // This value drives the display
  const [repositories, setRepositories] = useState<Repository[]>([]);
  // This holds the counts and percentages for the full set of repos
  const [repositoryStats, setRepositoryStats] = useState<UpdateStats>();
  // Controls the "shimmer" effect on the list
  const [isLoading, setIsLoading] = useState(false);

  const app = useAppContext();
  const msal = useMsal();

  // This effect runs on load to retrieve repos from
  // the service
  useEffect(() => {
    const loadRepositories = async () => {
      setIsLoading(true);
      try {
        // Get a token for the web API
        const result = await msal.instance.acquireTokenSilent({
          scopes: [`api://${authConfig.clientId}/Repos.Read`],
        });

        // Make the API call
        const response = await fetch(`repositories/${props.type}`, {
          headers: { Authorization: `Bearer ${result.accessToken}` },
        });

        if (response.ok) {
          const fetchedRepositories: Repository[] = await response.json();

          // Sort the results by name by default
          allRepositories.current = copyAndSort(
            fetchedRepositories,
            'name',
            false
          );

          // Set the repos in the view
          setRepositories(allRepositories.current);
          // Calculate and set the stats
          setRepositoryStats(getRepositoryStats(allRepositories.current));
        } else {
          app.displayError!(
            'An error occurred while loading repositories',
            `API returned ${response.status}`
          );
        }
      } catch (error: any) {
        app.displayError!(
          'An error occurred while loading repositories',
          error.toString()
        );
      }

      setIsLoading(false);
    };

    loadRepositories();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [msal.instance]);

  // Called as user types in the name text field
  const onFilterByName = (
    ev: React.FormEvent<HTMLInputElement | HTMLTextAreaElement>,
    text?: string
  ): void => {
    setRepositories(
      text
        ? allRepositories.current.filter(
            (r) => r.name.toLowerCase().indexOf(text.toLowerCase()) > -1
          )
        : allRepositories.current
    );
  };

  // Called as user types in the owner text field
  const onFilterByOwner = (
    ev: React.FormEvent<HTMLInputElement | HTMLTextAreaElement>,
    text?: string
  ): void => {
    setRepositories(
      text
        ? allRepositories.current.filter((r) => {
            // Check each owner for the value in the text field
            for (const owner of r.owners) {
              if (
                owner.displayName.toLowerCase().includes(text.toLowerCase())
              ) {
                return true;
              }
            }
            return false;
          })
        : allRepositories.current
    );
  };

  // Using useCallback here because this event handler
  // needs to be updated every time the repositories state value changes
  // otherwise, it always uses a stale copy
  const onColumnClick = useCallback(
    (ev: React.MouseEvent<HTMLElement>, column: IColumn): void => {
      const newColumns = columns.current.slice();
      const currentColumn = newColumns.filter(
        (currCol) => column.key === currCol.key
      )[0];
      newColumns.forEach((newColumn) => {
        if (newColumn === currentColumn) {
          newColumn.isSortedDescending = !newColumn.isSortedDescending;
          newColumn.isSorted = true;
        } else {
          newColumn.isSorted = false;
          newColumn.isSortedDescending = true;
        }
      });

      const newRepositories = copyAndSort(
        repositories,
        currentColumn.fieldName!,
        currentColumn.isSortedDescending
      );
      columns.current = newColumns;
      setRepositories(newRepositories);
    },
    [repositories]
  );

  // Add an effect here every time the onColumnClick value is changed
  // to update the handler assignment on the columns
  useEffect(() => {
    for (const column of columns.current) {
      column.onColumnClick = onColumnClick;
    }
  }, [onColumnClick]);

  // Collection of columns in the list
  const columns = useRef<IColumn[]>(repositoryColumns);

  return (
    <div>
      <StatusCardRow stats={repositoryStats} includeUnknowns={false} />
      <h3 className='ms-2 my-2'>Repositories ({repositories.length})</h3>
      <div className={classNames.wrapper}>
        <ScrollablePane scrollbarVisibility={ScrollbarVisibility.auto}>
          <Sticky stickyPosition={StickyPositionType.Header}>
            <Stack horizontal tokens={stackTokens} styles={stackStyles}>
              <Stack {...columnProps}>
                <TextField
                  className={filterListClass}
                  label='Filter by name:'
                  onChange={onFilterByName}
                  styles={textFieldStyles}
                />
              </Stack>
              <Stack {...columnProps}>
                <TextField
                  className={filterListClass}
                  label='Filter by owner:'
                  onChange={onFilterByOwner}
                  styles={textFieldStyles}
                />
              </Stack>
            </Stack>
          </Sticky>
          <ShimmeredDetailsList
            items={repositories}
            columns={columns.current}
            selectionMode={SelectionMode.none}
            layoutMode={DetailsListLayoutMode.justified}
            isHeaderVisible={true}
            onRenderItemColumn={renderItemColumn}
            enableShimmer={isLoading}
            onRenderDetailsHeader={(props, defaultRender) => {
              if (!props) {
                return null;
              }
              return (
                <Sticky
                  stickyPosition={StickyPositionType.Header}
                  isScrollSynced={true}
                >
                  {defaultRender!({ ...props })}
                </Sticky>
              );
            }}
          />
        </ScrollablePane>
      </div>
    </div>
  );
}
