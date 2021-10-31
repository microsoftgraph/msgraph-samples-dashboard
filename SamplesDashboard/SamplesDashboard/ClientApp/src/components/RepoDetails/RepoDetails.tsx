// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import {
  useMsal,
  AuthenticatedTemplate,
  UnauthenticatedTemplate,
} from '@azure/msal-react';
import { useCallback, useEffect, useRef, useState } from 'react';
import { Redirect, RouteComponentProps, useParams } from 'react-router-dom';
import {
  DefaultButton,
  DetailsListLayoutMode,
  FontIcon,
  FontSizes,
  IColumn,
  ScrollablePane,
  ScrollbarVisibility,
  SelectionMode,
  ShimmeredDetailsList,
  Stack,
  Sticky,
  StickyPositionType,
  TextField,
} from '@fluentui/react';

import { useAppContext } from '../../AppContext';
import Repository from '../../types/Repository';
import { classNames } from '../SharedStyles';
import { authConfig } from '../../AuthConfig';
import { getDependencyStats, UpdateStats } from '../../types/UpdateStats';
import StatusCardRow from '../StatusCardRow/StatusCardRow';
import StatusIndicator from '../StatusIndicator/StatusIndicator';
import Dependency from '../../types/Dependency';
import { dependencyColumns } from './DependencyColumns';
import copyAndSort, {
  processColumnSorting,
} from '../../utilities/copy-and-sort';
import { dependencyListColumns } from '../../strings/Strings';

interface RepoDetailsParams {
  name: string;
}

// Used by the ShimmeredDetailList to render a single
// column within a row
function renderItemColumn(item: Dependency, index?: number, column?: IColumn) {
  // Return the appropriate component based on the
  // column name
  switch (column?.name) {
    case dependencyListColumns.packageName:
      return <span>{item.packageName}</span>;

    case dependencyListColumns.currentVersion:
      return <span>{item.currentVersion}</span>;

    case dependencyListColumns.latestVersion:
      return <span>{item.latestVersion}</span>;

    case dependencyListColumns.status:
      return (
        <StatusIndicator
          status={item.status}
          objectName='dependency'
          mode='dependency'
          useUnknown={true}
        />
      );
  }
}

export default function RepoDetails(props: RouteComponentProps) {
  const [repository, setRepository] = useState<Repository>();
  const [dependencyStats, setDependencyStats] = useState<UpdateStats>();
  const [dependencies, setDependencies] = useState<Dependency[]>([]);
  // Controls the 'shimmer' effect on the list
  const [isLoading, setIsLoading] = useState(false);

  const app = useAppContext();
  const msal = useMsal();
  let params: RepoDetailsParams = useParams();

  useEffect(() => {
    const loadDetails = async () => {
      setIsLoading(true);
      try {
        // Get a token for the web API
        const result = await msal.instance.acquireTokenSilent({
          scopes: [`api://${authConfig.clientId}/Repos.Read`],
        });

        // Make the API call
        const response = await fetch(`/repositories/details/${params.name}`, {
          headers: { Authorization: `Bearer ${result.accessToken}` },
        });

        if (response.ok) {
          const fetchedRepository: Repository = await response.json();
          setRepository(fetchedRepository);
          setDependencies(
            fetchedRepository.dependencies
              ? copyAndSort(
                  fetchedRepository.dependencies,
                  'packageName',
                  false
                )
              : []
          );
          setDependencyStats(getDependencyStats(fetchedRepository));
        } else {
          app.displayError!(
            'An error occurred while fetching repository details',
            `API returned ${response.status}`
          );
        }
      } catch (error: any) {
        app.displayError!(
          'An error occurred while fetching repository details',
          error.toString()
        );
      }

      setIsLoading(false);
    };

    loadDetails();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [msal.instance]);

  // Called as user types in the name text field
  const onFilterByName = (
    ev: React.FormEvent<HTMLInputElement | HTMLTextAreaElement>,
    text?: string
  ): void => {
    const allDependencies = repository?.dependencies || [];
    setDependencies(
      text
        ? allDependencies.filter(
            (d) => d.packageName.toLowerCase().indexOf(text.toLowerCase()) > -1
          )
        : allDependencies
    );
  };

  // Using useCallback here because this event handler
  // needs to be updated every time the dependencies state value changes
  // otherwise, it always uses a stale copy
  const onColumnClick = useCallback(
    (ev: React.MouseEvent<HTMLElement>, column: IColumn): void => {
      const newColumns = columns.current.slice();
      const currentColumn = newColumns.filter(
        (currCol) => column.key === currCol.key
      )[0];
      processColumnSorting(newColumns, currentColumn);

      const newDependencies = copyAndSort(
        dependencies,
        currentColumn.fieldName!,
        currentColumn.isSortedDescending
      );
      columns.current = newColumns;
      setDependencies(newDependencies);
    },
    [dependencies]
  );

  // Add an effect here every time the onColumnClick value is changed
  // to update the handler assignment on the columns
  useEffect(() => {
    for (const column of columns.current) {
      column.onColumnClick = onColumnClick;
    }
  }, [onColumnClick]);

  // Collection of columns in the list
  const columns = useRef<IColumn[]>(dependencyColumns);

  return (
    <div>
      <AuthenticatedTemplate>
        <Stack horizontal>
          <h1 className='me-2'>{repository?.name}</h1>
          <DefaultButton style={{ marginBottom: '1em', marginTop: '1em' }}>
            <a href={repository?.url} target='_blank' rel='noopener noreferrer'>
              <FontIcon iconName='OpenInNewTab' /> Go to repository
            </a>
          </DefaultButton>
        </Stack>
        <p style={{ fontSize: FontSizes.size20 }}>{repository?.description}</p>
        <StatusCardRow stats={dependencyStats} includeUnknowns={true} />
        <h3 className='ms-2 my-2'>Dependencies ({dependencies?.length})</h3>
        <div className={classNames.wrapper}>
          <ScrollablePane scrollbarVisibility={ScrollbarVisibility.auto}>
            <Sticky stickyPosition={StickyPositionType.Header}>
              <TextField
                className={''}
                label='Filter by library:'
                onChange={onFilterByName}
                styles={{ root: { maxWidth: '300px' } }}
              />
            </Sticky>
            <ShimmeredDetailsList
              items={dependencies}
              columns={dependencyColumns}
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
      </AuthenticatedTemplate>
      <UnauthenticatedTemplate>
        <Redirect to='/' />
      </UnauthenticatedTemplate>
    </div>
  );
}
